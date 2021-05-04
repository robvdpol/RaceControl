using NLog;
using Prism.Commands;
using Prism.Events;
using Prism.Services.Dialogs;
using RaceControl.Common.Constants;
using RaceControl.Common.Enums;
using RaceControl.Common.Interfaces;
using RaceControl.Core.Helpers;
using RaceControl.Core.Mvvm;
using RaceControl.Core.Settings;
using RaceControl.Events;
using RaceControl.Extensions;
using RaceControl.Interfaces;
using RaceControl.Services.Interfaces.F1TV;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace RaceControl.ViewModels
{
    // ReSharper disable UnusedMember.Global
    public class VideoDialogViewModel : DialogViewModelBase
    {
        private const int MouseWheelDelta = 12;

        private readonly IEventAggregator _eventAggregator;
        private readonly ISettings _settings;
        private readonly IApiService _apiService;
        private readonly IVideoDialogLayout _videoDialogLayout;
        private readonly object _showControlsTimerLock = new();

        private ICommand _mouseDownCommand;
        private ICommand _mouseEnterOrLeaveOrMoveVideoCommand;
        private ICommand _mouseWheelVideoCommand;
        private ICommand _mouseEnterControlBarCommand;
        private ICommand _mouseLeaveControlBarCommand;
        private ICommand _mouseMoveControlBarCommand;
        private ICommand _mouseWheelControlBarCommand;
        private ICommand _togglePauseCommand;
        private ICommand _togglePauseAllCommand;
        private ICommand _toggleMuteCommand;
        private ICommand _fastForwardCommand;
        private ICommand _syncSessionCommand;
        private ICommand _toggleFullScreenCommand;
        private ICommand _moveToCornerCommand;
        private ICommand _zoomCommand;
        private ICommand _selectAspectRatioCommand;
        private ICommand _selectAudioDeviceCommand;
        private ICommand _closeVideoWindowCommand;
        private ICommand _closeAllWindowsCommand;

        private string _subscriptionToken;
        private long _identifier;
        private IPlayableContent _playableContent;
        private VideoDialogSettings _dialogSettings;
        private WindowStartupLocation _startupLocation = WindowStartupLocation.CenterOwner;
        private bool _isMouseOver;
        private bool _showControls = true;
        private bool _contextMenuIsOpen;
        private Timer _showControlsTimer;

        public VideoDialogViewModel(
            ILogger logger,
            IEventAggregator eventAggregator,
            ISettings settings,
            IApiService apiService,
            IVideoDialogLayout videoDialogLayout,
            IMediaPlayer mediaPlayer)
            : base(logger)
        {
            _eventAggregator = eventAggregator;
            _settings = settings;
            _apiService = apiService;
            _videoDialogLayout = videoDialogLayout;
            MediaPlayer = mediaPlayer;
        }

        public override string Title => $"{_identifier}. {PlayableContent?.Title}";

        public ICommand MouseDownCommand => _mouseDownCommand ??= new DelegateCommand<MouseButtonEventArgs>(MouseDownExecute);
        public ICommand MouseEnterOrLeaveOrMoveVideoCommand => _mouseEnterOrLeaveOrMoveVideoCommand ??= new DelegateCommand<bool?>(MouseEnterOrLeaveOrMoveVideoExecute);
        public ICommand MouseWheelVideoCommand => _mouseWheelVideoCommand ??= new DelegateCommand<MouseWheelEventArgs>(MouseWheelVideoExecute);
        public ICommand MouseEnterControlBarCommand => _mouseEnterControlBarCommand ??= new DelegateCommand(MouseEnterControlBarExecute);
        public ICommand MouseLeaveControlBarCommand => _mouseLeaveControlBarCommand ??= new DelegateCommand(MouseLeaveControlBarExecute);
        public ICommand MouseMoveControlBarCommand => _mouseMoveControlBarCommand ??= new DelegateCommand(MouseMoveControlBarExecute);
        public ICommand MouseWheelControlBarCommand => _mouseWheelControlBarCommand ??= new DelegateCommand<MouseWheelEventArgs>(MouseWheelControlBarExecute);
        public ICommand TogglePauseCommand => _togglePauseCommand ??= new DelegateCommand(TogglePauseExecute).ObservesCanExecute(() => MediaPlayer.IsStarted);
        public ICommand TogglePauseAllCommand => _togglePauseAllCommand ??= new DelegateCommand(TogglePauseAllExecute).ObservesCanExecute(() => MediaPlayer.IsStarted);
        public ICommand ToggleMuteCommand => _toggleMuteCommand ??= new DelegateCommand<bool?>(ToggleMuteExecute).ObservesCanExecute(() => MediaPlayer.IsStarted);
        public ICommand FastForwardCommand => _fastForwardCommand ??= new DelegateCommand<int?>(FastForwardExecute, CanFastForwardExecute).ObservesProperty(() => MediaPlayer.IsStarted).ObservesProperty(() => PlayableContent);
        public ICommand SyncSessionCommand => _syncSessionCommand ??= new DelegateCommand(SyncSessionExecute).ObservesCanExecute(() => MediaPlayer.IsStarted);
        public ICommand ToggleFullScreenCommand => _toggleFullScreenCommand ??= new DelegateCommand<long?>(ToggleFullScreenExecute).ObservesCanExecute(() => MediaPlayer.IsStarted);
        public ICommand MoveToCornerCommand => _moveToCornerCommand ??= new DelegateCommand<WindowLocation?>(MoveToCornerExecute, CanMoveToCornerExecute).ObservesProperty(() => MediaPlayer.IsFullScreen);
        public ICommand ZoomCommand => _zoomCommand ??= new DelegateCommand<int?>(ZoomExecute).ObservesCanExecute(() => MediaPlayer.IsStarted);
        public ICommand SelectAspectRatioCommand => _selectAspectRatioCommand ??= new DelegateCommand<IAspectRatio>(SelectAspectRatioExecute, CanSelectAspectRatioExecute).ObservesProperty(() => MediaPlayer.IsStarted).ObservesProperty(() => MediaPlayer.AspectRatio);
        public ICommand SelectAudioDeviceCommand => _selectAudioDeviceCommand ??= new DelegateCommand<IAudioDevice>(SelectAudioDeviceExecute, CanSelectAudioDeviceExecute).ObservesProperty(() => MediaPlayer.IsStarted).ObservesProperty(() => MediaPlayer.AudioDevice);
        public ICommand CloseVideoWindowCommand => _closeVideoWindowCommand ??= new DelegateCommand(RaiseRequestClose).ObservesCanExecute(() => MediaPlayer.IsStarted);
        public ICommand CloseAllWindowsCommand => _closeAllWindowsCommand ??= new DelegateCommand(CloseAllWindowsExecute).ObservesCanExecute(() => MediaPlayer.IsStarted);

        public IMediaPlayer MediaPlayer { get; }

        public IDictionary<VideoQuality, string> VideoQualities { get; } = new Dictionary<VideoQuality, string>
        {
            { VideoQuality.High, "High" },
            { VideoQuality.Medium, "Medium" },
            { VideoQuality.Low, "Low" },
            { VideoQuality.Lowest, "Potato" }
        };

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

        public bool IsMouseOver
        {
            get => _isMouseOver;
            set => SetProperty(ref _isMouseOver, value);
        }

        public bool ShowControls
        {
            get => _showControls;
            set => SetProperty(ref _showControls, value);
        }

        public bool ContextMenuIsOpen
        {
            get => _contextMenuIsOpen;
            set => SetProperty(ref _contextMenuIsOpen, value);
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            _subscriptionToken = parameters.GetValue<string>(ParameterNames.SubscriptionToken);
            _identifier = parameters.GetValue<long>(ParameterNames.Identifier);
            PlayableContent = parameters.GetValue<IPlayableContent>(ParameterNames.Content);

            var dialogSettings = parameters.GetValue<VideoDialogSettings>(ParameterNames.Settings);

            if (dialogSettings != null)
            {
                StartupLocation = WindowStartupLocation.Manual;
                LoadDialogSettings(dialogSettings);
            }
            else
            {
                StartupLocation = WindowStartupLocation.CenterScreen;
                DialogSettings.AudioTrack = DetermineAudioTrack();
            }

            StartStreamAsync().Await(StreamStarted, StreamFailed, true);
        }

        public override void OnDialogClosed()
        {
            base.OnDialogClosed();
            MediaPlayer.Dispose();
            RemoveShowControlsTimer();
            UnsubscribeEvents();
        }

        private void ShowControlsTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ShowControls = false;

                if (IsMouseOver && !ContextMenuIsOpen)
                {
                    Mouse.OverrideCursor = Cursors.None;
                }
            });
        }

        private void MouseDownExecute(MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left || e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            switch (e.ClickCount)
            {
                case 1:
                    if (e.Source is DependencyObject dependencyObject)
                    {
                        var window = Window.GetWindow(dependencyObject)?.Owner;

                        if (window != null)
                        {
                            window.Focus();
                            window.DragMove();
                        }
                    }

                    break;

                case 2:
                    ToggleFullScreenCommand.TryExecute();
                    break;
            }
        }

        private void MouseEnterOrLeaveOrMoveVideoExecute(bool? isMouseOver)
        {
            if (isMouseOver.HasValue)
            {
                IsMouseOver = isMouseOver.Value;
            }

            ShowControlsAndResetTimer();
        }

        private void MouseWheelVideoExecute(MouseWheelEventArgs e)
        {
            SetVolume(e.Delta / MouseWheelDelta);
            ShowControlsAndResetTimer();
        }

        private void MouseEnterControlBarExecute()
        {
            lock (_showControlsTimerLock)
            {
                _showControlsTimer?.Stop();
            }
        }

        private void MouseLeaveControlBarExecute()
        {
            lock (_showControlsTimerLock)
            {
                _showControlsTimer?.Start();
            }
        }

        private static void MouseMoveControlBarExecute()
        {
            if (Mouse.OverrideCursor != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Mouse.OverrideCursor = null;
                });
            }
        }

        private void MouseWheelControlBarExecute(MouseWheelEventArgs e)
        {
            SetVolume(e.Delta / MouseWheelDelta);
        }

        private void TogglePauseExecute()
        {
            Logger.Info("Toggling pause...");
            MediaPlayer.TogglePause();
        }

        private void TogglePauseAllExecute()
        {
            Logger.Info("Toggling pause for all video players...");
            _eventAggregator.GetEvent<PauseAllEvent>().Publish();
        }

        private void ToggleMuteExecute(bool? mute)
        {
            Logger.Info("Toggling mute...");
            MediaPlayer.ToggleMute(mute);
        }

        private bool CanFastForwardExecute(int? seconds)
        {
            return MediaPlayer.IsStarted && !PlayableContent.IsLive;
        }

        private void FastForwardExecute(int? seconds)
        {
            if (seconds.HasValue)
            {
                Logger.Info($"Fast forwarding stream {seconds.Value} seconds...");
                MediaPlayer.Time += TimeSpan.FromSeconds(seconds.Value).Ticks;
            }
        }

        private void SyncSessionExecute()
        {
            var payload = new SyncStreamsEventPayload(PlayableContent.SyncUID, MediaPlayer.Time);
            Logger.Info($"Syncing streams with sync-UID '{payload.SyncUID}' to timestamp '{payload.Time}'...");
            _eventAggregator.GetEvent<SyncStreamsEvent>().Publish(payload);
        }

        private void OnSyncStreams(SyncStreamsEventPayload payload)
        {
            if (MediaPlayer.IsStarted && PlayableContent.SyncUID == payload.SyncUID)
            {
                MediaPlayer.Time = payload.Time;
            }
        }

        private void OnPauseAll()
        {
            TogglePauseCommand.TryExecute();
        }

        private void OnMuteAll(long identifier)
        {
            var mute = identifier != _identifier;
            ToggleMuteCommand.TryExecute(mute);
        }

        private void OnCloseAll(ContentType? contentType)
        {
            CloseVideoWindowCommand.TryExecute();
        }

        private void OnSaveLayout(ContentType contentType)
        {
            var dialogSettings = GetDialogSettings();
            _videoDialogLayout.Instances.Add(dialogSettings);
        }

        private void OnToggleFullScreen(long identifier)
        {
            if (ToggleFullScreenCommand.TryExecute() && MediaPlayer.IsFullScreen)
            {
                _eventAggregator.GetEvent<MuteAllEvent>().Publish(identifier);
            }
        }

        private void ToggleFullScreenExecute(long? identifier)
        {
            if (identifier == null)
            {
                if (!MediaPlayer.IsFullScreen)
                {
                    DialogSettings.ResizeMode = ResizeMode.CanResize;
                }

                MediaPlayer.ToggleFullScreen();
            }
            else
            {
                _eventAggregator.GetEvent<ToggleFullScreenEvent>().Publish(identifier.Value);
            }
        }

        private bool CanMoveToCornerExecute(WindowLocation? location)
        {
            return !MediaPlayer.IsFullScreen && location != null;
        }

        private void MoveToCornerExecute(WindowLocation? location)
        {
            var screen = ScreenHelper.GetScreen(DialogSettings);
            var screenScale = ScreenHelper.GetScreenScale();
            var screenTop = screen.WorkingArea.Top / screenScale;
            var screenLeft = screen.WorkingArea.Left / screenScale;
            var windowLocation = location.GetValueOrDefault();
            var windowWidth = windowLocation.GetWindowWidthOrHeight(screen.WorkingArea.Width, screenScale);
            var windowHeight = windowLocation.GetWindowWidthOrHeight(screen.WorkingArea.Height, screenScale);
            windowLocation.GetWindowTopAndLeft(screenTop, screenLeft, windowWidth, windowHeight, out var windowTop, out var windowLeft);

            DialogSettings.ResizeMode = ResizeMode.NoResize;
            DialogSettings.Width = windowWidth;
            DialogSettings.Height = windowHeight;
            DialogSettings.Top = windowTop;
            DialogSettings.Left = windowLeft;
        }

        private void ZoomExecute(int? zoom)
        {
            if (zoom.HasValue)
            {
                MediaPlayer.Zoom += zoom.Value;
            }
            else
            {
                MediaPlayer.Zoom = 0;
            }
        }

        private bool CanSelectAspectRatioExecute(IAspectRatio aspectRatio)
        {
            return MediaPlayer.IsStarted && MediaPlayer.AspectRatio != aspectRatio;
        }

        private void SelectAspectRatioExecute(IAspectRatio aspectRatio)
        {
            MediaPlayer.AspectRatio = aspectRatio;
        }

        private bool CanSelectAudioDeviceExecute(IAudioDevice audioDevice)
        {
            return MediaPlayer.IsStarted && MediaPlayer.AudioDevice != audioDevice;
        }

        private void SelectAudioDeviceExecute(IAudioDevice audioDevice)
        {
            MediaPlayer.AudioDevice = audioDevice;
        }

        private void CloseAllWindowsExecute()
        {
            _eventAggregator.GetEvent<CloseAllEvent>().Publish(null);
        }

        private void LoadDialogSettings(VideoDialogSettings settings)
        {
            // Properties need to be set in this order
            DialogSettings.FullScreen = settings.FullScreen;
            DialogSettings.ResizeMode = settings.ResizeMode;
            DialogSettings.Topmost = settings.Topmost;
            DialogSettings.Top = settings.Top;
            DialogSettings.Left = settings.Left;

            if (!settings.FullScreen)
            {
                DialogSettings.Width = settings.Width;
                DialogSettings.Height = settings.Height;
            }

            DialogSettings.VideoQuality = settings.VideoQuality;
            DialogSettings.IsMuted = settings.IsMuted;
            DialogSettings.Volume = settings.Volume;
            DialogSettings.Zoom = settings.Zoom;
            DialogSettings.AspectRatio = settings.AspectRatio;
            DialogSettings.AudioDevice = settings.AudioDevice;
            DialogSettings.AudioTrack = settings.AudioTrack;
        }

        private VideoDialogSettings GetDialogSettings()
        {
            return new()
            {
                Top = DialogSettings.Top,
                Left = DialogSettings.Left,
                Width = DialogSettings.Width,
                Height = DialogSettings.Height,
                FullScreen = MediaPlayer.IsFullScreen,
                ResizeMode = DialogSettings.ResizeMode,
                VideoQuality = MediaPlayer.VideoQuality,
                Topmost = DialogSettings.Topmost,
                IsMuted = MediaPlayer.IsMuted,
                Volume = MediaPlayer.Volume,
                Zoom = MediaPlayer.Zoom,
                AspectRatio = MediaPlayer.AspectRatio?.Value,
                AudioDevice = MediaPlayer.AudioDevice?.Identifier,
                AudioTrack = MediaPlayer.AudioTrack?.Id,
                ChannelName = PlayableContent.Name
            };
        }

        private string DetermineAudioTrack()
        {
            if (PlayableContent.ContentType != ContentType.Channel || PlayableContent.Name == ChannelNames.PitLane)
            {
                return LanguageCodes.English;
            }

            if (PlayableContent.Name is ChannelNames.Wif or ChannelNames.Tracker or ChannelNames.Data)
            {
                return !string.IsNullOrWhiteSpace(_settings.DefaultAudioLanguage) ? _settings.DefaultAudioLanguage : LanguageCodes.English;
            }

            return LanguageCodes.Undetermined;
        }

        private async Task StartStreamAsync()
        {
            var streamUrl = await _apiService.GetTokenisedUrlAsync(_subscriptionToken, PlayableContent);

            if (string.IsNullOrWhiteSpace(streamUrl))
            {
                throw new Exception("An error occurred while retrieving the stream URL.");
            }

            MediaPlayer.StartPlayback(streamUrl, DialogSettings);
        }

        private void StreamStarted()
        {
            base.OnDialogOpened(null);
            SubscribeEvents();
            CreateShowControlsTimer();
        }

        private void StreamFailed(Exception ex)
        {
            base.OnDialogOpened(null);
            RaiseRequestClose();
            HandleCriticalError(ex);
        }

        private void SubscribeEvents()
        {
            _eventAggregator.GetEvent<SyncStreamsEvent>().Subscribe(OnSyncStreams);
            _eventAggregator.GetEvent<PauseAllEvent>().Subscribe(OnPauseAll);
            _eventAggregator.GetEvent<MuteAllEvent>().Subscribe(OnMuteAll);
            _eventAggregator.GetEvent<CloseAllEvent>().Subscribe(OnCloseAll, contentType => contentType == null || contentType == PlayableContent.ContentType);
            _eventAggregator.GetEvent<SaveLayoutEvent>().Subscribe(OnSaveLayout, contentType => contentType == PlayableContent.ContentType);
            _eventAggregator.GetEvent<ToggleFullScreenEvent>().Subscribe(OnToggleFullScreen, identifier => identifier == _identifier);
        }

        private void UnsubscribeEvents()
        {
            _eventAggregator.GetEvent<SyncStreamsEvent>().Unsubscribe(OnSyncStreams);
            _eventAggregator.GetEvent<PauseAllEvent>().Unsubscribe(OnPauseAll);
            _eventAggregator.GetEvent<MuteAllEvent>().Unsubscribe(OnMuteAll);
            _eventAggregator.GetEvent<CloseAllEvent>().Unsubscribe(OnCloseAll);
            _eventAggregator.GetEvent<SaveLayoutEvent>().Unsubscribe(OnSaveLayout);
            _eventAggregator.GetEvent<ToggleFullScreenEvent>().Unsubscribe(OnToggleFullScreen);
        }

        private void CreateShowControlsTimer()
        {
            lock (_showControlsTimerLock)
            {
                _showControlsTimer = new Timer(2000) { AutoReset = false };
                _showControlsTimer.Elapsed += ShowControlsTimer_Elapsed;
                _showControlsTimer.Start();
            }
        }

        private void RemoveShowControlsTimer()
        {
            lock (_showControlsTimerLock)
            {
                if (_showControlsTimer != null)
                {
                    _showControlsTimer.Stop();
                    _showControlsTimer.Dispose();
                    _showControlsTimer = null;
                }
            }
        }

        private void ShowControlsAndResetTimer()
        {
            lock (_showControlsTimerLock)
            {
                _showControlsTimer?.Stop();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    ShowControls = true;

                    if (Mouse.OverrideCursor != null)
                    {
                        Mouse.OverrideCursor = null;
                    }
                });

                _showControlsTimer?.Start();
            }
        }

        private void SetVolume(int delta)
        {
            MediaPlayer.Volume += delta;
        }
    }
}