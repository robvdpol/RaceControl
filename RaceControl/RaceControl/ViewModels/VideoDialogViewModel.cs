using Timer = System.Timers.Timer;

namespace RaceControl.ViewModels;

// ReSharper disable UnusedMember.Global
public class VideoDialogViewModel : DialogViewModelBase
{
    private const int MouseWheelDelta = 12;

    private readonly IEventAggregator _eventAggregator;
    private readonly ISettings _settings;
    private readonly IApiService _apiService;
    private readonly IVideoDialogLayout _videoDialogLayout;
    private readonly object _showControlsTimerLock = new();
    private readonly object _showNotificationTimerLock = new();

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
    private ICommand _showMainWindowCommand;
    private ICommand _toggleRecordingCommand;
    private ICommand _toggleFullScreenCommand;
    private ICommand _moveToCornerCommand;
    private ICommand _setZoomCommand;
    private ICommand _setSpeedCommand;
    private ICommand _selectAspectRatioCommand;
    private ICommand _selectAudioDeviceCommand;
    private ICommand _exitFullScreenOrCloseWindowCommand;
    private ICommand _closeAllWindowsCommand;
    private ICommand _windowStateChangedCommand;

    private long _identifier;
    private IPlayableContent _playableContent;
    private VideoDialogSettings _dialogSettings;
    private WindowStartupLocation _startupLocation = WindowStartupLocation.CenterOwner;
    private ResizeMode _windowedResizeMode;
    private bool _isMouseOver;
    private bool _showControls = true;
    private bool _showNotificationText;
    private bool _contextMenuIsOpen;
    private string _notificationText;
    private Timer _showControlsTimer;
    private Timer _showNotificationTimer;

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
    public ICommand TogglePauseAllCommand => _togglePauseAllCommand ??= new DelegateCommand(TogglePauseAllExecute);
    public ICommand ToggleMuteCommand => _toggleMuteCommand ??= new DelegateCommand<bool?>(ToggleMuteExecute).ObservesCanExecute(() => MediaPlayer.IsStarted);
    public ICommand FastForwardCommand => _fastForwardCommand ??= new DelegateCommand<int?>(FastForwardExecute, CanFastForwardExecute).ObservesProperty(() => MediaPlayer.IsStarted).ObservesProperty(() => PlayableContent);
    public ICommand SyncSessionCommand => _syncSessionCommand ??= new DelegateCommand(SyncSessionExecute, CanSyncSessionExecute).ObservesProperty(() => MediaPlayer.IsStarted).ObservesProperty(() => PlayableContent);
    public ICommand ShowMainWindowCommand => _showMainWindowCommand ??= new DelegateCommand(ShowMainWindowExecute);
    public ICommand ToggleRecordingCommand => _toggleRecordingCommand ??= new DelegateCommand(ToggleRecordingExecute).ObservesCanExecute(() => MediaPlayer.IsStarted);
    public ICommand ToggleFullScreenCommand => _toggleFullScreenCommand ??= new DelegateCommand<long?>(ToggleFullScreenExecute);
    public ICommand MoveToCornerCommand => _moveToCornerCommand ??= new DelegateCommand<WindowLocation?>(MoveToCornerExecute, CanMoveToCornerExecute).ObservesProperty(() => DialogSettings.FullScreen);
    public ICommand SetZoomCommand => _setZoomCommand ??= new DelegateCommand<int?>(SetZoomExecute).ObservesCanExecute(() => MediaPlayer.IsStarted);
    public ICommand SetSpeedCommand => _setSpeedCommand ??= new DelegateCommand<bool?>(SetSpeedExecute, CanSetSpeedExecute).ObservesProperty(() => MediaPlayer.IsStarted).ObservesProperty(() => PlayableContent);
    public ICommand SelectAspectRatioCommand => _selectAspectRatioCommand ??= new DelegateCommand<IAspectRatio>(SelectAspectRatioExecute, CanSelectAspectRatioExecute).ObservesProperty(() => MediaPlayer.IsStarted).ObservesProperty(() => MediaPlayer.AspectRatio);
    public ICommand SelectAudioDeviceCommand => _selectAudioDeviceCommand ??= new DelegateCommand<IAudioDevice>(SelectAudioDeviceExecute, CanSelectAudioDeviceExecute).ObservesProperty(() => MediaPlayer.IsStarted).ObservesProperty(() => MediaPlayer.AudioDevice);
    public ICommand ExitFullScreenOrCloseWindowCommand => _exitFullScreenOrCloseWindowCommand ??= new DelegateCommand(ExitFullScreenOrCloseWindowExecute);
    public ICommand CloseAllWindowsCommand => _closeAllWindowsCommand ??= new DelegateCommand(CloseAllWindowsExecute);
    public ICommand WindowStateChangedCommand => _windowStateChangedCommand ??= new DelegateCommand<Window>(WindowStateChangedExecute);

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

    public ResizeMode WindowedResizeMode
    {
        get => _windowedResizeMode;
        set => SetProperty(ref _windowedResizeMode, value);
    }

    public string NotificationText
    {
        get => _notificationText;
        set => SetProperty(ref _notificationText, value);
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

    public bool ShowNotificationText
    {
        get => _showNotificationText;
        set => SetProperty(ref _showNotificationText, value);
    }

    public bool ContextMenuIsOpen
    {
        get => _contextMenuIsOpen;
        set => SetProperty(ref _contextMenuIsOpen, value);
    }

    public override void OnDialogOpened(IDialogParameters parameters)
    {
        _identifier = parameters.GetValue<long>(ParameterNames.Identifier);
        PlayableContent = parameters.GetValue<IPlayableContent>(ParameterNames.Content);

        var dialogSettings = parameters.GetValue<VideoDialogSettings>(ParameterNames.Settings);

        if (dialogSettings != null)
        {
            StartupLocation = WindowStartupLocation.Manual;
            WindowedResizeMode = dialogSettings.ResizeMode;
            LoadDialogSettings(dialogSettings);
        }
        else
        {
            StartupLocation = WindowStartupLocation.CenterScreen;
            WindowedResizeMode = DialogSettings.ResizeMode;
            DialogSettings.VideoQuality = _settings.DefaultVideoQuality;
            DialogSettings.AudioTrack = PlayableContent.GetPreferredAudioLanguage(_settings.DefaultAudioLanguage);
        }

        StartStreamAsync().Await(StreamStarted, StreamFailed, true);
    }

    public override void OnDialogClosed()
    {
        base.OnDialogClosed();
        RemoveShowControlsTimer();
        RemoveShowNotificationTimer();
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

    private void ShowNotificationTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            ShowNotificationText = false;
            NotificationText = null;
        });
    }

    private void MouseDownExecute(MouseButtonEventArgs e)
    {
        switch (e.ChangedButton)
        {
            case MouseButton.Middle when e.MiddleButton == MouseButtonState.Pressed:
                SetVolume(100);
                break;

            case MouseButton.Left when e.LeftButton == MouseButtonState.Pressed:
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
        AddVolume(e.Delta / MouseWheelDelta);
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
        AddVolume(e.Delta / MouseWheelDelta);
    }

    private void TogglePauseExecute()
    {
        MediaPlayer.TogglePause();
    }

    private void TogglePauseAllExecute()
    {
        _eventAggregator.GetEvent<PauseAllEvent>().Publish();
    }

    private void ToggleMuteExecute(bool? mute)
    {
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
            MediaPlayer.FastForward(seconds.Value);
        }
    }

    private bool CanSyncSessionExecute()
    {
        return MediaPlayer.IsStarted && !PlayableContent.IsLive;
    }

    private void SyncSessionExecute()
    {
        var payload = new SyncStreamsEventPayload(PlayableContent.SyncUID, MediaPlayer.GetCurrentTime());
        _eventAggregator.GetEvent<SyncStreamsEvent>().Publish(payload);
    }

    private static void ShowMainWindowExecute()
    {
        var mainWindow = Application.Current.MainWindow;

        if (mainWindow != null)
        {
            mainWindow.WindowState = WindowState.Normal;
            mainWindow.Focus();
        }
    }

    private void ToggleRecordingExecute()
    {
        if (!MediaPlayer.IsRecording)
        {
            var filename = $"{DateTime.Now:yyyyMMddHHmmss} {PlayableContent.Title}".RemoveInvalidFileNameChars();

            if (!string.IsNullOrWhiteSpace(_settings.RecordingLocation))
            {
                filename = Path.Combine(_settings.RecordingLocation, filename);
            }

            MediaPlayer.StartRecording(filename);
        }
        else
        {
            MediaPlayer.StopRecording();
        }
    }

    private void ToggleFullScreenExecute(long? identifier)
    {
        if (identifier == null)
        {
            if (!DialogSettings.FullScreen)
            {
                SetFullScreen();
            }
            else
            {
                SetWindowed();
            }
        }
        else
        {
            _eventAggregator.GetEvent<ToggleFullScreenEvent>().Publish(identifier.Value);
        }
    }

    private bool CanMoveToCornerExecute(WindowLocation? location)
    {
        return !DialogSettings.FullScreen && location != null;
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

    private void SetZoomExecute(int? zoom)
    {
        if (zoom.HasValue)
        {
            MediaPlayer.Zoom += zoom.Value;
        }
        else
        {
            MediaPlayer.Zoom = 0;
        }

        ShowNotification($"Zoom: {MediaPlayer.Zoom}");
    }

    private bool CanSetSpeedExecute(bool? speedUp)
    {
        return MediaPlayer.IsStarted && !PlayableContent.IsLive;
    }

    private void SetSpeedExecute(bool? speedUp)
    {
        if (speedUp.HasValue)
        {
            if (speedUp.Value)
            {
                MediaPlayer.SpeedUp();
            }
            else
            {
                MediaPlayer.SpeedDown();
            }
        }
        else
        {
            MediaPlayer.Speed = 1;
        }

        ShowNotification($"Speed: {MediaPlayer.Speed:0.##}x");
    }

    private bool CanSelectAspectRatioExecute(IAspectRatio aspectRatio)
    {
        return MediaPlayer.IsStarted && MediaPlayer.AspectRatio != aspectRatio;
    }

    private void SelectAspectRatioExecute(IAspectRatio aspectRatio)
    {
        MediaPlayer.AspectRatio = aspectRatio;
        ShowNotification($"Aspect Ratio: {MediaPlayer.AspectRatio.Description}");
    }

    private bool CanSelectAudioDeviceExecute(IAudioDevice audioDevice)
    {
        return MediaPlayer.IsStarted && MediaPlayer.AudioDevice != audioDevice;
    }

    private void SelectAudioDeviceExecute(IAudioDevice audioDevice)
    {
        MediaPlayer.AudioDevice = audioDevice;
        ShowNotification($"Audio Device: {MediaPlayer.AudioDevice.Description}");
    }

    private void ExitFullScreenOrCloseWindowExecute()
    {
        if (DialogSettings.FullScreen)
        {
            ToggleFullScreenCommand.TryExecute();
        }
        else
        {
            CloseWindowCommand.TryExecute();
        }
    }

    private void CloseAllWindowsExecute()
    {
        _eventAggregator.GetEvent<CloseAllEvent>().Publish(null);
    }

    private void WindowStateChangedExecute(Window window)
    {
        if (window.WindowState == WindowState.Maximized && DialogSettings.ResizeMode != ResizeMode.NoResize)
        {
            SetWindowed();
            SetFullScreen();
        }

        if (window.WindowState == WindowState.Normal && DialogSettings.ResizeMode != WindowedResizeMode)
        {
            SetWindowed();
        }
    }

    private void OnSyncStreams(SyncStreamsEventPayload payload)
    {
        if (MediaPlayer.IsStarted && PlayableContent.SyncUID == payload.SyncUID)
        {
            MediaPlayer.SetCurrentTime(payload.Time);
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
        CloseWindowCommand.TryExecute();
    }

    private void OnSaveLayout(ContentType contentType)
    {
        var dialogSettings = GetDialogSettings();
        _videoDialogLayout.Instances.Add(dialogSettings);
    }

    private void OnToggleFullScreen(long identifier)
    {
        if (ToggleFullScreenCommand.TryExecute() && DialogSettings.FullScreen)
        {
            _eventAggregator.GetEvent<MuteAllEvent>().Publish(identifier);
        }
    }

    private void LoadDialogSettings(VideoDialogSettings settings)
    {
        // Properties need to be set in this order
        if (settings.FullScreen)
        {
            SetFullScreen();
        }
        else
        {
            SetWindowed();
        }

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
            FullScreen = DialogSettings.FullScreen,
            ResizeMode = DialogSettings.ResizeMode,
            VideoQuality = MediaPlayer.VideoQuality,
            Topmost = DialogSettings.Topmost,
            IsMuted = MediaPlayer.IsMuted,
            Volume = MediaPlayer.Volume,
            Zoom = MediaPlayer.Zoom,
            AspectRatio = MediaPlayer.AspectRatio?.Value,
            AudioDevice = MediaPlayer.AudioDevice?.Identifier,
            AudioTrack = LanguageCodes.GetStandardCode(MediaPlayer.AudioTrack?.Id),
            ChannelName = PlayableContent.Name
        };
    }

    private async Task StartStreamAsync()
    {
        var streamUrl = await _apiService.GetTokenisedUrlAsync(_settings.SubscriptionToken, PlayableContent);

        if (string.IsNullOrWhiteSpace(streamUrl))
        {
            throw new Exception("An error occurred while retrieving the stream URL.");
        }

        var playToken = await _apiService.GetPlayTokenAsync(streamUrl);
        await MediaPlayer.StartPlaybackAsync(streamUrl, playToken, DialogSettings);
    }

    private void StreamStarted()
    {
        base.OnDialogOpened(null);
        SubscribeEvents();
        CreateShowControlsTimer();
        CreateShowNotificationTimer();
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

    private void CreateShowNotificationTimer()
    {
        lock (_showNotificationTimerLock)
        {
            _showNotificationTimer = new Timer(2000) { AutoReset = false };
            _showNotificationTimer.Elapsed += ShowNotificationTimer_Elapsed;
            _showNotificationTimer.Start();
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

    private void RemoveShowNotificationTimer()
    {
        lock (_showNotificationTimerLock)
        {
            if (_showNotificationTimer != null)
            {
                _showNotificationTimer.Stop();
                _showNotificationTimer.Dispose();
                _showNotificationTimer = null;
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

    private void SetFullScreen()
    {
        WindowedResizeMode = DialogSettings.ResizeMode;
        DialogSettings.ResizeMode = ResizeMode.NoResize;
        DialogSettings.FullScreen = true;
    }

    private void SetWindowed()
    {
        DialogSettings.ResizeMode = WindowedResizeMode;
        DialogSettings.FullScreen = false;
    }

    private void SetVolume(int volume)
    {
        if (MediaPlayer.IsStarted)
        {
            MediaPlayer.Volume = volume;
            ShowNotification($"Volume: {volume}%");
        }
    }

    private void AddVolume(int delta)
    {
        SetVolume(MediaPlayer.Volume + delta);
    }

    private void ShowNotification(string text)
    {
        NotificationText = text;
        ShowNotificationText = true;

        lock (_showNotificationTimerLock)
        {
            _showNotificationTimer?.Stop();
            _showNotificationTimer?.Start();
        }
    }
}