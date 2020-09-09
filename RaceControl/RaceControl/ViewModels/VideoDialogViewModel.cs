using NLog;
using Prism.Commands;
using Prism.Events;
using Prism.Services.Dialogs;
using RaceControl.Common.Enums;
using RaceControl.Common.Interfaces;
using RaceControl.Core.Mvvm;
using RaceControl.Core.Settings;
using RaceControl.Core.Streamlink;
using RaceControl.Events;
using RaceControl.Services.Interfaces.F1TV;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Application = System.Windows.Application;
using Cursors = System.Windows.Input.Cursors;
using Screen = System.Windows.Forms.Screen;
using Timer = System.Timers.Timer;

namespace RaceControl.ViewModels
{
    public class VideoDialogViewModel : DialogViewModelBase, IVideoDialogViewModel
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IApiService _apiService;
        private readonly IStreamlinkLauncher _streamlinkLauncher;
        private readonly ISettings _settings;

        private ICommand _mouseDownVideoCommand;
        private ICommand _mouseMoveVideoCommand;
        private ICommand _mouseEnterVideoCommand;
        private ICommand _mouseLeaveVideoCommand;
        private ICommand _mouseMoveControlBarCommand;
        private ICommand _mouseEnterControlBarCommand;
        private ICommand _mouseLeaveControlBarCommand;
        private ICommand _togglePauseCommand;
        private ICommand _toggleMuteCommand;
        private ICommand _fastForwardCommand;
        private ICommand _syncSessionCommand;
        private ICommand _toggleRecordingCommand;
        private ICommand _toggleFullScreenCommand;
        private ICommand _moveToCornerCommand;
        private ICommand _audioTrackSelectionChangedCommand;
        private ICommand _scanChromecastCommand;
        private ICommand _startCastVideoCommand;
        private ICommand _stopCastVideoCommand;

        private Process _streamlinkProcess;
        private Process _streamlinkRecordingProcess;
        private string _token;
        private IPlayableContent _playableContent;
        private VideoDialogSettings _dialogSettings;
        private WindowStartupLocation _startupLocation = WindowStartupLocation.CenterOwner;
        private bool _isStreamlink;
        private bool _isRecording;
        private bool _showControls = true;
        private IMediaRenderer _selectedMediaRenderer;
        private Timer _showControlsTimer;
        private SubscriptionToken _syncStreamsEventToken;
        private string _carImageUrl;

        private string _headshotImageUrl;

        public VideoDialogViewModel(
            ILogger logger,
            IEventAggregator eventAggregator,
            IApiService apiService,
            IStreamlinkLauncher streamlinkLauncher,
            ISettings settings,
            IMediaPlayer mediaPlayer)
            : base(logger)
        {
            _eventAggregator = eventAggregator;
            _apiService = apiService;
            _streamlinkLauncher = streamlinkLauncher;
            _settings = settings;
            MediaPlayer = mediaPlayer;
        }

        public override string Title => PlayableContent?.Title;

        public ICommand MouseDownVideoCommand => _mouseDownVideoCommand ??= new DelegateCommand<MouseButtonEventArgs>(MouseDownVideoExecute);
        public ICommand MouseMoveVideoCommand => _mouseMoveVideoCommand ??= new DelegateCommand(MouseEnterOrLeaveOrMoveVideoExecute);
        public ICommand MouseEnterVideoCommand => _mouseEnterVideoCommand ??= new DelegateCommand(MouseEnterOrLeaveOrMoveVideoExecute);
        public ICommand MouseLeaveVideoCommand => _mouseLeaveVideoCommand ??= new DelegateCommand(MouseEnterOrLeaveOrMoveVideoExecute);
        public ICommand MouseMoveControlBarCommand => _mouseMoveControlBarCommand ??= new DelegateCommand(MouseMoveControlBarExecute);
        public ICommand MouseEnterControlBarCommand => _mouseEnterControlBarCommand ??= new DelegateCommand(MouseEnterControlBarExecute);
        public ICommand MouseLeaveControlBarCommand => _mouseLeaveControlBarCommand ??= new DelegateCommand(MouseLeaveControlBarExecute);
        public ICommand TogglePauseCommand => _togglePauseCommand ??= new DelegateCommand(TogglePauseExecute).ObservesCanExecute(() => CanClose);
        public ICommand ToggleMuteCommand => _toggleMuteCommand ??= new DelegateCommand(ToggleMuteExecute).ObservesCanExecute(() => CanClose);
        public ICommand FastForwardCommand => _fastForwardCommand ??= new DelegateCommand<string>(FastForwardExecute, CanFastForwardExecute).ObservesProperty(() => CanClose).ObservesProperty(() => PlayableContent);
        public ICommand SyncSessionCommand => _syncSessionCommand ??= new DelegateCommand(SyncSessionExecute, CanSyncSessionExecute).ObservesProperty(() => CanClose).ObservesProperty(() => PlayableContent);
        public ICommand ToggleRecordingCommand => _toggleRecordingCommand ??= new DelegateCommand(ToggleRecordingExecute, CanToggleRecordingExecute).ObservesProperty(() => CanClose).ObservesProperty(() => PlayableContent).ObservesProperty(() => IsRecording).ObservesProperty(() => MediaPlayer.IsPaused);
        public ICommand ToggleFullScreenCommand => _toggleFullScreenCommand ??= new DelegateCommand(ToggleFullScreenExecute);
        public ICommand MoveToCornerCommand => _moveToCornerCommand ??= new DelegateCommand<WindowLocation?>(MoveToCornerExecute, CanMoveToCornerExecute).ObservesProperty(() => DialogSettings.WindowState);
        public ICommand AudioTrackSelectionChangedCommand => _audioTrackSelectionChangedCommand ??= new DelegateCommand<SelectionChangedEventArgs>(AudioTrackSelectionChangedExecute);
        public ICommand ScanChromecastCommand => _scanChromecastCommand ??= new DelegateCommand(ScanChromecastExecute, CanScanChromecastExecute).ObservesProperty(() => CanClose).ObservesProperty(() => MediaPlayer.IsScanning);
        public ICommand StartCastVideoCommand => _startCastVideoCommand ??= new DelegateCommand(StartCastVideoExecute, CanStartCastVideoExecute).ObservesProperty(() => CanClose).ObservesProperty(() => SelectedMediaRenderer);
        public ICommand StopCastVideoCommand => _stopCastVideoCommand ??= new DelegateCommand(StopCastVideoExecute, CanStopCastVideoExecute).ObservesProperty(() => MediaPlayer.IsCasting);

        public Guid UniqueIdentifier { get; } = Guid.NewGuid();

        public IMediaPlayer MediaPlayer { get; }

        public IPlayableContent PlayableContent
        {
            get => _playableContent;
            set => SetProperty(ref _playableContent, value);
        }

        public VideoDialogSettings DialogSettings
        {
            get => _dialogSettings ??= VideoDialogSettings.GetDefaultSettings();
            set => SetProperty(ref _dialogSettings, value);
        }

        public WindowStartupLocation StartupLocation
        {
            get => _startupLocation;
            set => SetProperty(ref _startupLocation, value);
        }

        public bool IsStreamlink
        {
            get => _isStreamlink;
            set => SetProperty(ref _isStreamlink, value);
        }

        public bool IsRecording
        {
            get => _isRecording;
            set => SetProperty(ref _isRecording, value);
        }

        public bool ShowControls
        {
            get => _showControls;
            set => SetProperty(ref _showControls, value);
        }

        public IMediaRenderer SelectedMediaRenderer
        {
            get => _selectedMediaRenderer;
            set => SetProperty(ref _selectedMediaRenderer, value);
        }

        public string CarImageUrl
        {
            get => _carImageUrl;
            set => SetProperty(ref _carImageUrl, value);
        }

        public string HeadshotImageUrl
        {
            get => _headshotImageUrl;
            set => SetProperty(ref _headshotImageUrl, value);
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            _token = parameters.GetValue<string>(ParameterNames.TOKEN);
            PlayableContent = parameters.GetValue<IPlayableContent>(ParameterNames.CONTENT);
            IsStreamlink = PlayableContent.IsLive && !_settings.DisableStreamlink;

            var dialogSettings = parameters.GetValue<VideoDialogSettings>(ParameterNames.SETTINGS);

            if (dialogSettings != null)
            {
                StartupLocation = WindowStartupLocation.Manual;
                LoadDialogSettings(dialogSettings);
            }
            else
            {
                StartupLocation = WindowStartupLocation.CenterScreen;
            }

            StartStreamAsync().Await(SubscribeSyncStreamsEvent, HandleCriticalError);
            LoadDriverImageUrlsAsync().Await(HandleNonCriticalError);
            StartShowControlsTimer();

            base.OnDialogOpened(parameters);
        }

        public override void OnDialogClosed()
        {
            MediaPlayer.Dispose();
            RemoveShowControlsTimer();
            UnsubscribeSyncStreamsEvent();
            CleanupProcess(_streamlinkProcess);
            CleanupProcess(_streamlinkRecordingProcess);

            base.OnDialogClosed();
        }

        public VideoDialogSettings GetDialogSettings()
        {
            DialogSettings.ChannelName = PlayableContent.Name;
            DialogSettings.IsMuted = MediaPlayer.IsMuted;

            return DialogSettings;
        }

        private void ShowControlsTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ShowControls = false;

            Application.Current.Dispatcher.Invoke(() =>
            {
                Mouse.OverrideCursor = Cursors.None;
            });
        }

        private void MouseDownVideoExecute(MouseButtonEventArgs args)
        {
            if (args.ChangedButton != MouseButton.Left)
            {
                return;
            }

            switch (args.ClickCount)
            {
                case 1:
                    if (args.Source is DependencyObject dependencyObject)
                    {
                        Window.GetWindow(dependencyObject)?.Owner?.DragMove();
                    }

                    break;

                case 2:
                    if (ToggleFullScreenCommand.CanExecute(null))
                    {
                        ToggleFullScreenCommand.Execute(null);
                    }

                    break;
            }
        }

        private void MouseEnterOrLeaveOrMoveVideoExecute()
        {
            _showControlsTimer?.Stop();
            ShowControls = true;

            Application.Current.Dispatcher.Invoke(() =>
            {
                Mouse.OverrideCursor = null;
            });

            _showControlsTimer?.Start();
        }

        private static void MouseMoveControlBarExecute()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Mouse.OverrideCursor = null;
            });
        }

        private void MouseEnterControlBarExecute()
        {
            _showControlsTimer?.Stop();
        }

        private void MouseLeaveControlBarExecute()
        {
            _showControlsTimer?.Start();
        }

        private void TogglePauseExecute()
        {
            Logger.Info("Toggling pause...");
            MediaPlayer.TogglePause();
        }

        private void ToggleMuteExecute()
        {
            Logger.Info("Toggling mute...");
            MediaPlayer.ToggleMute();
        }

        private bool CanFastForwardExecute(string arg)
        {
            return CanClose && !PlayableContent.IsLive;
        }

        private void FastForwardExecute(string value)
        {
            if (int.TryParse(value, out var seconds))
            {
                Logger.Info($"Fast forwarding stream {seconds} seconds...");
                MediaPlayer.Time += seconds * 1000;
            }
        }

        private bool CanSyncSessionExecute()
        {
            return CanClose && !PlayableContent.IsLive;
        }

        private void SyncSessionExecute()
        {
            var payload = new SyncStreamsEventPayload(PlayableContent.SyncUID, MediaPlayer.Time);
            Logger.Info($"Syncing streams with sync-UID '{payload.SyncUID}' to timestamp '{payload.Time}'...");
            _eventAggregator.GetEvent<SyncStreamsEvent>().Publish(payload);
        }

        private bool CanToggleRecordingExecute()
        {
            return CanClose && PlayableContent.IsLive && (IsRecording || !MediaPlayer.IsPaused);
        }

        private void ToggleRecordingExecute()
        {
            if (!IsRecording)
            {
                StartRecordingAsync().Await(() => IsRecording = true, HandleNonCriticalError);
            }
            else
            {
                StopRecording();
                IsRecording = false;
            }
        }

        private void OnSyncSession(SyncStreamsEventPayload payload)
        {
            if (CanClose && PlayableContent.SyncUID == payload.SyncUID && !PlayableContent.IsLive)
            {
                MediaPlayer.Time = payload.Time;
            }
        }

        private void ToggleFullScreenExecute()
        {
            if (DialogSettings.WindowState != WindowState.Maximized)
            {
                SetFullScreen();
            }
            else
            {
                SetWindowed();
            }
        }

        private bool CanMoveToCornerExecute(WindowLocation? location)
        {
            return DialogSettings.WindowState != WindowState.Maximized && location != null;
        }

        private void MoveToCornerExecute(WindowLocation? location)
        {
            Logger.Info($"Moving window to corner '{location}'...");
            var screen = Screen.FromRectangle(new Rectangle((int)DialogSettings.Left, (int)DialogSettings.Top, (int)DialogSettings.Width, (int)DialogSettings.Height));
            var scale = Math.Max(Screen.PrimaryScreen.WorkingArea.Width / SystemParameters.PrimaryScreenWidth, Screen.PrimaryScreen.WorkingArea.Height / SystemParameters.PrimaryScreenHeight);
            var top = screen.WorkingArea.Top / scale;
            var left = screen.WorkingArea.Left / scale;
            var width = screen.WorkingArea.Width / 2D / scale;
            var height = screen.WorkingArea.Height / 2D / scale;
            DialogSettings.ResizeMode = ResizeMode.NoResize;

            switch (location)
            {
                case WindowLocation.TopLeft:
                    DialogSettings.Top = top;
                    DialogSettings.Left = left;
                    break;

                case WindowLocation.TopRight:
                    DialogSettings.Top = top;
                    DialogSettings.Left = left + width;
                    break;

                case WindowLocation.BottomLeft:
                    DialogSettings.Top = top + height;
                    DialogSettings.Left = left;
                    break;

                case WindowLocation.BottomRight:
                    DialogSettings.Top = top + height;
                    DialogSettings.Left = left + width;
                    break;
            }

            DialogSettings.Width = width;
            DialogSettings.Height = height;
        }

        private void AudioTrackSelectionChangedExecute(SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is IMediaTrack audioTrack)
            {
                Logger.Info($"Changing audio track to '{audioTrack.Name}'...");
                MediaPlayer.SetAudioTrack(audioTrack);
            }
        }

        private bool CanScanChromecastExecute()
        {
            return CanClose && !MediaPlayer.IsScanning;
        }

        private void ScanChromecastExecute()
        {
            Logger.Info("Scanning for Chromecast devices...");
            MediaPlayer.ScanChromecastAsync().Await(HandleNonCriticalError);
        }

        private bool CanStartCastVideoExecute()
        {
            return CanClose && SelectedMediaRenderer != null;
        }

        private void StartCastVideoExecute()
        {
            Logger.Info($"Starting casting of video with renderer '{SelectedMediaRenderer.Name}'...");
            ChangeRendererAsync(SelectedMediaRenderer).Await(HandleNonCriticalError);
        }

        private bool CanStopCastVideoExecute()
        {
            return MediaPlayer.IsCasting;
        }

        private void StopCastVideoExecute()
        {
            Logger.Info("Stopping casting of video...");
            ChangeRendererAsync().Await(HandleNonCriticalError);
        }

        private void LoadDialogSettings(VideoDialogSettings settings)
        {
            // Properties need to be set in this order
            DialogSettings.ResizeMode = settings.ResizeMode;
            DialogSettings.WindowState = settings.WindowState;
            DialogSettings.Topmost = settings.Topmost;
            DialogSettings.Top = settings.Top;
            DialogSettings.Left = settings.Left;

            if (settings.WindowState != WindowState.Maximized)
            {
                DialogSettings.Width = settings.Width;
                DialogSettings.Height = settings.Height;
            }

            DialogSettings.IsMuted = settings.IsMuted;
        }

        private async Task StartStreamAsync()
        {
            var streamUrl = await _apiService.GetTokenisedUrlAsync(_token, PlayableContent);

            if (IsStreamlink)
            {
                var (streamlinkProcess, streamlinkUrl) = await _streamlinkLauncher.StartStreamlinkExternalAsync(streamUrl);
                _streamlinkProcess = streamlinkProcess;
                streamUrl = streamlinkUrl;
            }

            await MediaPlayer.StartPlaybackAsync(streamUrl);

            if (MediaPlayer.IsMuted != DialogSettings.IsMuted)
            {
                MediaPlayer.ToggleMute();
            }
        }

        private async Task LoadDriverImageUrlsAsync()
        {
            if (string.IsNullOrWhiteSpace(PlayableContent.DriverUID))
            {
                return;
            }

            var driver = await _apiService.GetDriverAsync(PlayableContent.DriverUID);

            if (driver != null)
            {
                CarImageUrl = driver.CarUrl;
                HeadshotImageUrl = driver.HeadshotUrl;
            }
        }

        private void SubscribeSyncStreamsEvent()
        {
            if (_syncStreamsEventToken != null)
            {
                return;
            }

            _syncStreamsEventToken = _eventAggregator.GetEvent<SyncStreamsEvent>().Subscribe(OnSyncSession);
        }

        private void UnsubscribeSyncStreamsEvent()
        {
            if (_syncStreamsEventToken == null)
            {
                return;
            }

            _eventAggregator.GetEvent<SyncStreamsEvent>().Unsubscribe(_syncStreamsEventToken);
            _syncStreamsEventToken = null;
        }

        private void StartShowControlsTimer()
        {
            if (_showControlsTimer != null)
            {
                return;
            }

            _showControlsTimer = new Timer(2000) { AutoReset = false };
            _showControlsTimer.Elapsed += ShowControlsTimer_Elapsed;
            _showControlsTimer.Start();
        }

        private void RemoveShowControlsTimer()
        {
            if (_showControlsTimer == null)
            {
                return;
            }

            _showControlsTimer.Stop();
            _showControlsTimer.Dispose();
            _showControlsTimer = null;
        }

        private async Task ChangeRendererAsync(IMediaRenderer mediaRenderer = null)
        {
            Logger.Info($"Changing renderer to '{mediaRenderer?.Name}'...");
            string streamUrl = null;

            if (!IsStreamlink)
            {
                streamUrl = await _apiService.GetTokenisedUrlAsync(_token, PlayableContent);
            }

            var time = MediaPlayer.Time;
            await MediaPlayer.ChangeRendererAsync(mediaRenderer, streamUrl);

            if (!PlayableContent.IsLive)
            {
                MediaPlayer.Time = time;
            }
        }

        private async Task StartRecordingAsync()
        {
            Logger.Info("Starting recording process...");
            var streamUrl = await _apiService.GetTokenisedUrlAsync(_token, PlayableContent);
            _streamlinkRecordingProcess = _streamlinkLauncher.StartStreamlinkRecording(streamUrl, PlayableContent.Title);
        }

        private void StopRecording()
        {
            Logger.Info("Stopping recording process...");
            CleanupProcess(_streamlinkRecordingProcess);
            _streamlinkRecordingProcess = null;
        }

        private void SetFullScreen()
        {
            Logger.Info("Changing to fullscreen mode...");
            DialogSettings.ResizeMode = ResizeMode.NoResize;
            DialogSettings.WindowState = WindowState.Maximized;
        }

        private void SetWindowed()
        {
            Logger.Info("Changing to windowed mode...");
            DialogSettings.WindowState = WindowState.Normal;
        }

        private void CleanupProcess(Process process)
        {
            if (process != null)
            {
                if (!process.HasExited)
                {
                    try
                    {
                        process.Kill(true);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, $"An error occurred while trying to kill process with id '{process.Id}' and name '{process.ProcessName}'.");
                    }
                }

                process.Dispose();
            }
        }
    }
}