using LibVLCSharp.Shared;
using LibVLCSharp.Shared.Structures;
using NLog;
using Prism.Commands;
using Prism.Events;
using Prism.Services.Dialogs;
using RaceControl.Common.Interfaces;
using RaceControl.Common.Settings;
using RaceControl.Core.Mvvm;
using RaceControl.Enums;
using RaceControl.Events;
using RaceControl.Interfaces;
using RaceControl.Services.Interfaces.F1TV;
using RaceControl.Streamlink;
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
        private IPlayableContent _playableContent;
        private string _token;
        private bool _isStreamlink;
        private bool _isRecording;
        private bool _showControls;
        private RendererItem _selectedRendererItem;
        private Timer _showControlsTimer;
        private SubscriptionToken _syncStreamsEventToken;
        private double _top;
        private double _left;
        private double _width = 960;
        private double _height = 550;
        private ResizeMode _resizeMode = ResizeMode.CanResize;
        private WindowState _windowState = WindowState.Normal;
        private WindowStartupLocation _startupLocation = WindowStartupLocation.CenterOwner;
        private bool _topmost;
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

        public ICommand MouseDownVideoCommand => _mouseDownVideoCommand ??= new DelegateCommand<MouseButtonEventArgs>(MouseDownVideoExecute);
        public ICommand MouseMoveVideoCommand => _mouseMoveVideoCommand ??= new DelegateCommand(MouseEnterOrLeaveOrMoveVideoExecute);
        public ICommand MouseEnterVideoCommand => _mouseEnterVideoCommand ??= new DelegateCommand(MouseEnterOrLeaveOrMoveVideoExecute);
        public ICommand MouseLeaveVideoCommand => _mouseLeaveVideoCommand ??= new DelegateCommand(MouseEnterOrLeaveOrMoveVideoExecute);
        public ICommand MouseMoveControlBarCommand => _mouseMoveControlBarCommand ??= new DelegateCommand(MouseMoveControlBarExecute);
        public ICommand MouseEnterControlBarCommand => _mouseEnterControlBarCommand ??= new DelegateCommand(MouseEnterControlBarExecute);
        public ICommand MouseLeaveControlBarCommand => _mouseLeaveControlBarCommand ??= new DelegateCommand(MouseLeaveControlBarExecute);
        public ICommand TogglePauseCommand => _togglePauseCommand ??= new DelegateCommand(TogglePauseExecute).ObservesCanExecute(() => Initialized);
        public ICommand ToggleMuteCommand => _toggleMuteCommand ??= new DelegateCommand(ToggleMuteExecute).ObservesCanExecute(() => Initialized);
        public ICommand FastForwardCommand => _fastForwardCommand ??= new DelegateCommand<string>(FastForwardExecute, CanFastForwardExecute).ObservesProperty(() => Initialized).ObservesProperty(() => PlayableContent);
        public ICommand SyncSessionCommand => _syncSessionCommand ??= new DelegateCommand(SyncSessionExecute, CanSyncSessionExecute).ObservesProperty(() => Initialized).ObservesProperty(() => PlayableContent);
        public ICommand ToggleRecordingCommand => _toggleRecordingCommand ??= new DelegateCommand(ToggleRecordingExecute, CanToggleRecordingExecute).ObservesProperty(() => Initialized).ObservesProperty(() => PlayableContent).ObservesProperty(() => IsRecording).ObservesProperty(() => MediaPlayer.IsPaused);
        public ICommand ToggleFullScreenCommand => _toggleFullScreenCommand ??= new DelegateCommand(ToggleFullScreenExecute);
        public ICommand MoveToCornerCommand => _moveToCornerCommand ??= new DelegateCommand<WindowLocation?>(MoveToCornerExecute, CanMoveToCornerExecute).ObservesProperty(() => WindowState);
        public ICommand AudioTrackSelectionChangedCommand => _audioTrackSelectionChangedCommand ??= new DelegateCommand<SelectionChangedEventArgs>(AudioTrackSelectionChangedExecute);
        public ICommand ScanChromecastCommand => _scanChromecastCommand ??= new DelegateCommand(ScanChromecastExecute, CanScanChromecastExecute).ObservesProperty(() => Initialized).ObservesProperty(() => MediaPlayer.IsScanning);
        public ICommand StartCastVideoCommand => _startCastVideoCommand ??= new DelegateCommand(StartCastVideoExecute, CanStartCastVideoExecute).ObservesProperty(() => Initialized).ObservesProperty(() => SelectedRendererItem);
        public ICommand StopCastVideoCommand => _stopCastVideoCommand ??= new DelegateCommand(StopCastVideoExecute, CanStopCastVideoExecute).ObservesProperty(() => MediaPlayer.IsCasting);

        public override string Title => PlayableContent?.Title;

        public Guid UniqueIdentifier { get; } = Guid.NewGuid();

        public IMediaPlayer MediaPlayer { get; }

        public IPlayableContent PlayableContent
        {
            get => _playableContent;
            set => SetProperty(ref _playableContent, value);
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

        public RendererItem SelectedRendererItem
        {
            get => _selectedRendererItem;
            set => SetProperty(ref _selectedRendererItem, value);
        }

        public double Top
        {
            get => _top;
            set => SetProperty(ref _top, value);
        }

        public double Left
        {
            get => _left;
            set => SetProperty(ref _left, value);
        }

        public double Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        public double Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        public ResizeMode ResizeMode
        {
            get => _resizeMode;
            set => SetProperty(ref _resizeMode, value);
        }

        public WindowState WindowState
        {
            get => _windowState;
            set => SetProperty(ref _windowState, value);
        }

        public WindowStartupLocation StartupLocation
        {
            get => _startupLocation;
            set => SetProperty(ref _startupLocation, value);
        }

        public bool Topmost
        {
            get => _topmost;
            set => SetProperty(ref _topmost, value);
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

        public VideoDialogInstance GetVideoDialogInstance()
        {
            return new VideoDialogInstance
            {
                Top = Top,
                Left = Left,
                Width = Width,
                Height = Height,
                ResizeMode = (int)ResizeMode,
                WindowState = (int)WindowState,
                Topmost = Topmost,
                ChannelName = PlayableContent.Name
            };
        }

        public override async void OnDialogOpened(IDialogParameters parameters)
        {
            _token = parameters.GetValue<string>(ParameterNames.TOKEN);
            PlayableContent = parameters.GetValue<IPlayableContent>(ParameterNames.PLAYABLE_CONTENT);
            IsStreamlink = PlayableContent.IsLive && !_settings.DisableStreamlink;

            var instance = parameters.GetValue<VideoDialogInstance>(ParameterNames.INSTANCE);
            SetWindowLocation(instance);

            var streamUrl = await GenerateStreamUrlAsync();

            if (streamUrl == null)
            {
                Logger.Error("Closing video player, stream URL is empty.");
                CloseWindow(true);
                return;
            }

            if (IsStreamlink)
            {
                var (streamlinkProcess, streamlinkUrl) = await _streamlinkLauncher.StartStreamlinkExternal(streamUrl);
                _streamlinkProcess = streamlinkProcess;
                streamUrl = streamlinkUrl;
            }

            await MediaPlayer.StartPlaybackAsync(streamUrl);

            _showControlsTimer = new Timer(2000) { AutoReset = false };
            _showControlsTimer.Elapsed += ShowControlsTimer_Elapsed;
            _showControlsTimer.Start();
            _syncStreamsEventToken = _eventAggregator.GetEvent<SyncStreamsEvent>().Subscribe(OnSyncSession);

            await LoadDriverImageUrlsAsync();

            base.OnDialogOpened(parameters);
        }

        public override void OnDialogClosed()
        {
            if (_syncStreamsEventToken != null)
            {
                _eventAggregator.GetEvent<SyncStreamsEvent>().Unsubscribe(_syncStreamsEventToken);
            }

            if (_showControlsTimer != null)
            {
                _showControlsTimer.Stop();
                _showControlsTimer.Dispose();
                _showControlsTimer = null;
            }

            MediaPlayer.Dispose();

            CleanupProcess(_streamlinkProcess);
            CleanupProcess(_streamlinkRecordingProcess);

            base.OnDialogClosed();
        }

        protected override void CloseWindowExecute()
        {
            var parameters = new DialogParameters
            {
                { ParameterNames.UNIQUE_IDENTIFIER, UniqueIdentifier }
            };

            RaiseRequestClose(new DialogResult(ButtonResult.None, parameters));
        }

        private void ShowControlsTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ShowControls = false;
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

            Application.Current.Dispatcher.Invoke(() =>
            {
                ShowControls = true;
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
            return Initialized && !PlayableContent.IsLive;
        }

        private void FastForwardExecute(string value)
        {
            if (int.TryParse(value, out var seconds))
            {
                Logger.Info($"Fast forwarding stream {seconds} seconds...");
                MediaPlayer.Time = MediaPlayer.Time + seconds * 1000;
            }
        }

        private bool CanSyncSessionExecute()
        {
            return Initialized && !PlayableContent.IsLive;
        }

        private void SyncSessionExecute()
        {
            var payload = new SyncStreamsEventPayload(PlayableContent.SyncUID, MediaPlayer.Time);
            Logger.Info($"Syncing streams with sync-UID '{payload.SyncUID}' to timestamp '{payload.Time}'...");
            _eventAggregator.GetEvent<SyncStreamsEvent>().Publish(payload);
        }

        private bool CanToggleRecordingExecute()
        {
            return Initialized && PlayableContent.IsLive && (IsRecording || !MediaPlayer.IsPaused);
        }

        private async void ToggleRecordingExecute()
        {
            if (!IsRecording)
            {
                IsRecording = await StartRecordingAsync();
            }
            else
            {
                StopRecording();
                IsRecording = false;
            }
        }

        private void OnSyncSession(SyncStreamsEventPayload payload)
        {
            if (Initialized && PlayableContent.SyncUID == payload.SyncUID && !PlayableContent.IsLive)
            {
                MediaPlayer.Time = payload.Time;
            }
        }

        private void ToggleFullScreenExecute()
        {
            if (WindowState != WindowState.Maximized)
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
            return WindowState != WindowState.Maximized && location != null;
        }

        private void MoveToCornerExecute(WindowLocation? location)
        {
            Logger.Info($"Moving window to corner '{location}'...");

            var screen = Screen.FromRectangle(new Rectangle((int)Left, (int)Top, (int)Width, (int)Height));
            var scale = Math.Max(Screen.PrimaryScreen.WorkingArea.Width / SystemParameters.PrimaryScreenWidth, Screen.PrimaryScreen.WorkingArea.Height / SystemParameters.PrimaryScreenHeight);
            var top = screen.WorkingArea.Top / scale;
            var left = screen.WorkingArea.Left / scale;
            var width = screen.WorkingArea.Width / 2D / scale;
            var height = screen.WorkingArea.Height / 2D / scale;

            ResizeMode = ResizeMode.NoResize;

            switch (location)
            {
                case WindowLocation.TopLeft:
                    Top = top;
                    Left = left;
                    break;

                case WindowLocation.TopRight:
                    Top = top;
                    Left = left + width;
                    break;

                case WindowLocation.BottomLeft:
                    Top = top + height;
                    Left = left;
                    break;

                case WindowLocation.BottomRight:
                    Top = top + height;
                    Left = left + width;
                    break;
            }

            Width = width;
            Height = height;

            Logger.Info($"Done moving window (top: {Top}, left: {Left}, width: {Width}, height: {Height}).");
        }

        private async void AudioTrackSelectionChangedExecute(SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is TrackDescription trackDescription)
            {
                Logger.Info($"Changing audio track to '{trackDescription.Id}'...");
                MediaPlayer.SetAudioTrack(trackDescription.Id);

                if (!IsStreamlink)
                {
                    // Workaround to fix audio out of sync after switching audio track
                    await Task.Delay(TimeSpan.FromMilliseconds(250));
                    MediaPlayer.Time = MediaPlayer.Time - 500;
                }

                Logger.Info("Done changing audio track.");
            }
        }

        private bool CanScanChromecastExecute()
        {
            return Initialized && !MediaPlayer.IsScanning;
        }

        private async void ScanChromecastExecute()
        {
            Logger.Info("Scanning for Chromecast devices...");
            await MediaPlayer.ScanChromecastAsync();
            Logger.Info("Done scanning for Chromecast devices.");
        }

        private bool CanStartCastVideoExecute()
        {
            return Initialized && SelectedRendererItem != null;
        }

        private async void StartCastVideoExecute()
        {
            Logger.Info($"Starting casting of video with renderer '{SelectedRendererItem.Name}'...");
            await ChangeRendererAsync(SelectedRendererItem);
        }

        private bool CanStopCastVideoExecute()
        {
            return MediaPlayer.IsCasting;
        }

        private async void StopCastVideoExecute()
        {
            Logger.Info("Stopping casting of video...");
            await ChangeRendererAsync(null);
        }

        private void SetWindowLocation(VideoDialogInstance instance)
        {
            if (instance != null)
            {
                StartupLocation = WindowStartupLocation.Manual;
                ResizeMode = (ResizeMode)instance.ResizeMode;
                WindowState = (WindowState)instance.WindowState;
                Topmost = instance.Topmost;
                Top = instance.Top;
                Left = instance.Left;

                if (WindowState != WindowState.Maximized)
                {
                    Width = instance.Width;
                    Height = instance.Height;
                }
            }
            else
            {
                StartupLocation = WindowStartupLocation.CenterScreen;
            }
        }

        private async Task<string> GenerateStreamUrlAsync()
        {
            try
            {
                return await _apiService.GetTokenisedUrlAsync(_token, PlayableContent);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "An error occurred while trying to get tokenised URL.");
            }

            return null;
        }

        private async Task LoadDriverImageUrlsAsync()
        {
            if (string.IsNullOrWhiteSpace(PlayableContent.DriverUID))
            {
                return;
            }

            try
            {
                var driver = await _apiService.GetDriverAsync(PlayableContent.DriverUID);

                if (driver != null)
                {
                    CarImageUrl = driver.CarUrl;
                    HeadshotImageUrl = driver.HeadshotUrl;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "An error occurred while trying to get driver images.");
            }
        }

        private async Task ChangeRendererAsync(RendererItem renderer)
        {
            Logger.Info($"Changing renderer to '{renderer?.Name}'...");

            var time = MediaPlayer.Time;

            if (IsStreamlink)
            {
                await MediaPlayer.ChangeRendererAsync(renderer);
            }
            else
            {
                var streamUrl = await GenerateStreamUrlAsync();

                if (streamUrl == null)
                {
                    Logger.Error("Renderer not changed, stream URL is empty.");
                    return;
                }

                await MediaPlayer.ChangeRendererAsync(renderer, streamUrl);
            }

            if (!PlayableContent.IsLive)
            {
                MediaPlayer.Time = time;
            }

            Logger.Info("Done changing renderer.");
        }

        private async Task<bool> StartRecordingAsync()
        {
            Logger.Info("Starting recording process...");
            var streamUrl = await GenerateStreamUrlAsync();

            if (streamUrl == null)
            {
                Logger.Error("Recording process not started, stream URL is empty.");
                return false;
            }

            _streamlinkRecordingProcess = _streamlinkLauncher.StartStreamlinkRecording(streamUrl, PlayableContent.Title);
            Logger.Info("Recording process started.");

            return true;
        }

        private void StopRecording()
        {
            Logger.Info("Stopping recording process...");
            CleanupProcess(_streamlinkRecordingProcess);
            _streamlinkRecordingProcess = null;
            Logger.Info("Recording process stopped.");
        }

        private void SetFullScreen()
        {
            Logger.Info("Changing to fullscreen mode...");
            ResizeMode = ResizeMode.NoResize;
            WindowState = WindowState.Maximized;
        }

        private void SetWindowed()
        {
            Logger.Info("Changing to windowed mode...");
            WindowState = WindowState.Normal;
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