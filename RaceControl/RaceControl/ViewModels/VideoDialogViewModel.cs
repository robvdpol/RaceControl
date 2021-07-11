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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json;
using Application = System.Windows.Application;
using Cursors = System.Windows.Input.Cursors;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Timer = System.Timers.Timer;

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


        private ICommand _keyDownCommand;
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
        private ICommand _selectChannelCommand;
        private ICommand _closeVideoWindowCommand;
        private ICommand _exitFullScreenOrCloseWindowCommand;
        private ICommand _closeAllWindowsCommand;
        private ICommand _windowStateChangedCommand;

        private long _identifier;
        private IPlayableContent _playableContent;
        private VideoDialogSettings _dialogSettings;
        private WindowStartupLocation _startupLocation = WindowStartupLocation.CenterOwner;
        private bool _isMouseOver;
        private bool _showControls = true;
        private bool _contextMenuIsOpen;
        private Timer _showControlsTimer;
        private readonly IReadOnlyCollection<(ModifierKeys Modifiers, Key Key, Func<Task> Action)> _boundHotkeys;


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

            _boundHotkeys = HookUpHotkeys();
        }
        
        public override string Title => $"{_identifier}. {PlayableContent?.Title}";

        public ICommand KeyDownCommand => _keyDownCommand ??= new DelegateCommand<KeyEventArgs>(KeyDownExecute);
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
        public ICommand ToggleFullScreenCommand => _toggleFullScreenCommand ??= new DelegateCommand<long?>(ToggleFullScreenExecute);
        public ICommand MoveToCornerCommand => _moveToCornerCommand ??= new DelegateCommand<WindowLocation?>(MoveToCornerExecute, CanMoveToCornerExecute).ObservesProperty(() => DialogSettings.FullScreen);
        public ICommand ZoomCommand => _zoomCommand ??= new DelegateCommand<int?>(ZoomExecute).ObservesCanExecute(() => MediaPlayer.IsStarted);
        public ICommand SelectAspectRatioCommand => _selectAspectRatioCommand ??= new DelegateCommand<IAspectRatio>(SelectAspectRatioExecute, CanSelectAspectRatioExecute).ObservesProperty(() => MediaPlayer.IsStarted).ObservesProperty(() => MediaPlayer.AspectRatio);
        public ICommand SelectAudioDeviceCommand => _selectAudioDeviceCommand ??= new DelegateCommand<IAudioDevice>(SelectAudioDeviceExecute, CanSelectAudioDeviceExecute).ObservesProperty(() => MediaPlayer.IsStarted).ObservesProperty(() => MediaPlayer.AudioDevice);
        public ICommand CloseVideoWindowCommand => _closeVideoWindowCommand ??= new DelegateCommand(RaiseRequestClose, CanCloseVideoWindowExecute).ObservesProperty(() => MediaPlayer.IsStarting);
        public ICommand ExitFullScreenOrCloseWindowCommand => _exitFullScreenOrCloseWindowCommand ??= new DelegateCommand(ExitFullScreenOrCloseWindowExecute);
        public ICommand CloseAllWindowsCommand => _closeAllWindowsCommand ??= new DelegateCommand(CloseAllWindowsExecute);
        public ICommand WindowStateChangedCommand => _windowStateChangedCommand ??= new DelegateCommand<Window>(WindowStateChangedExecute);
        public ICommand SelectChannelCommand => _selectChannelCommand ??= new DelegateCommand<IPlayableChannel>(SelectChannelExecute, CanSelectChannelExecute).ObservesProperty(() => Channels.CurrentChannel);

        public IMediaPlayer MediaPlayer { get; }

        public IChannelCollection Channels { get; private set; }

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
            set
            {
                SetProperty(ref _playableContent, value);
                RaisePropertyChanged(nameof(Title));
            }
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
            _identifier = parameters.GetValue<long>(ParameterNames.Identifier);
            PlayableContent = parameters.GetValue<IPlayableContent>(ParameterNames.Content);
            Channels = new ChannelCollection(parameters.GetValue<ObservableCollection<IPlayableChannel>>(ParameterNames.Channels), PlayableContent as IPlayableChannel);

            var dialogSettings = parameters.GetValue<VideoDialogSettings>(ParameterNames.Settings);

            if (dialogSettings != null)
            {
                StartupLocation = WindowStartupLocation.Manual;
                LoadDialogSettings(dialogSettings);
            }
            else
            {
                StartupLocation = WindowStartupLocation.CenterScreen;

                // TODO: not for onboards
                DialogSettings.AudioTrack = PlayableContent.GetPreferredAudioLanguage(_settings.DefaultAudioLanguage);
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

        private readonly Key[] _keysToIgnore = { Key.LeftShift, Key.RightShift, Key.LeftCtrl, Key.RightCtrl, Key.LeftAlt, Key.RightAlt, Key.System, Key.LWin, Key.RWin };
        private async void KeyDownExecute(KeyEventArgs args)
        {
            if (args.Handled || _keysToIgnore.Contains(args.Key))
            {
                return;
            }

            foreach (var hotkey in _boundHotkeys)
            {
                if (hotkey.Modifiers == Keyboard.Modifiers && hotkey.Key == args.Key)
                {
                    // Fire that action!
                    await hotkey.Action();
                }
            }
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

        private bool CanSyncSessionExecute()
        {
            return MediaPlayer.IsStarted && !PlayableContent.IsLive;
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
            if (ToggleFullScreenCommand.TryExecute() && DialogSettings.FullScreen)
            {
                _eventAggregator.GetEvent<MuteAllEvent>().Publish(identifier);
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

        private bool CanSelectChannelExecute(IPlayableChannel channel)
        {
            return Channels.CurrentChannel != channel;
        }

        private async void SelectChannelExecute(IPlayableChannel channel)
        {
            await ChangeChannelAsync(channel);
        }

        private async Task ChangeChannelAsync(IPlayableChannel channel)
        {
            var currentTime = MediaPlayer.Time;
            MediaPlayer.StopPlayback();

            Channels.CurrentChannel = channel;
            PlayableContent = channel;

            if (!PlayableContent.IsLive)
            {
                DialogSettings.StartTime = currentTime;
            }

            await StartStreamAsync();
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

        private bool CanCloseVideoWindowExecute()
        {
            return !MediaPlayer.IsStarting;
        }

        private void ExitFullScreenOrCloseWindowExecute()
        {
            if (DialogSettings.FullScreen)
            {
                ToggleFullScreenCommand.TryExecute();
            }
            else
            {
                CloseVideoWindowCommand.TryExecute();
            }
        }

        private void CloseAllWindowsExecute()
        {
            _eventAggregator.GetEvent<CloseAllEvent>().Publish(null);
        }

        private void WindowStateChangedExecute(Window window)
        {
            // Needed to set the resizemode to 'NoResize' when going fullscreen using a Windows key combination
            if (window.WindowState == WindowState.Maximized && DialogSettings.ResizeMode != ResizeMode.NoResize)
            {
                SetWindowed();
                SetFullScreen();
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
                SetWindowed(settings.ResizeMode);
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

            DialogSettings.IsMuted = Channels.Graphs.Contains(PlayableContent as IPlayableChannel);

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

        private void SetFullScreen()
        {
            DialogSettings.ResizeMode = ResizeMode.NoResize;
            DialogSettings.FullScreen = true;
        }

        private void SetWindowed(ResizeMode? resizeMode = null)
        {
            if (resizeMode.HasValue)
            {
                DialogSettings.ResizeMode = resizeMode.Value;
            }

            DialogSettings.FullScreen = false;
        }

        private void SetVolume(int volume)
        {
            if (MediaPlayer.IsStarted)
            {
                MediaPlayer.Volume = volume;
            }
        }

        private void AddVolume(int delta)
        {
            SetVolume(MediaPlayer.Volume + delta);
        }

        /// <summary>
        /// Reads hotkeys from settings and maps them to async actions.
        /// </summary>
        /// <returns></returns>
        private IReadOnlyCollection<(ModifierKeys, Key, Func<Task>)> HookUpHotkeys()
        {
            var boundHotkeys = new List<(ModifierKeys, Key, Func<Task>)>();

            foreach (var hotkey in _settings.Hotkeys)
            {
                var (modifiers, key) = GetKeyAndModifiers(hotkey);
                if (key == null)
                {
                    continue;
                }

                if (hotkey.Action == "SelectChannel")
                {
                    boundHotkeys.Add((modifiers, key.Value, async () =>
                    {
                        var channel = FindChannel(hotkey.Parameters);
                        if (channel != null)
                        {
                            await ChangeChannelAsync(channel);
                        }
                        else
                        {
                            Logger.Info($"Could not find channel '{hotkey.Parameters[0]}.{hotkey.Parameters[1]}' bound to key {hotkey.Key}");
                        }
                    }));
                }
            }

            return boundHotkeys.AsReadOnly();
        }

        private (ModifierKeys modifiers, Key? key) GetKeyAndModifiers(HotkeyBinding hotkey)
        {
            var controlIndex = hotkey.Key.IndexOf("^", StringComparison.Ordinal);
            var shiftlIndex = hotkey.Key.IndexOf("+", StringComparison.Ordinal);
            var altIndex = hotkey.Key.IndexOf("&", StringComparison.Ordinal);

            var modifiers = (controlIndex > -1 ? ModifierKeys.Control : 0)
                            | (shiftlIndex > -1 ? ModifierKeys.Shift : 0)
                            | (altIndex > -1 ? ModifierKeys.Alt : 0);

            var modifierIndex = Math.Max(controlIndex, Math.Max(shiftlIndex, altIndex));

            var keyString = hotkey.Key.Substring(modifierIndex > -1 ? modifierIndex + 1 : 0);

            if (!Enum.TryParse(keyString, out Key key))
            {
                Logger.Info($"Could not parse key '{hotkey.Key}' ('{keyString}')");
                return (modifiers, null);
            }

            return (modifiers, key);
        }

        /// <summary>
        /// Turn an array of [ "$groupName", "$channelName" ] into a channel, when found.
        /// </summary>
        private IPlayableChannel FindChannel(string[] parameters)
        {
            static bool ChannelEquals(IPlayableChannel channel, string channelName)
            {
                return channel.Name.Equals(channelName, StringComparison.InvariantCultureIgnoreCase)
                    || channel.DisplayName.Equals(channelName, StringComparison.InvariantCultureIgnoreCase);
            }

            var groupName = parameters[0];
            var channelName = parameters[1];

            return groupName switch
            {
                nameof(ChannelType.Global) => Channels.Globals.FirstOrDefault(c => ChannelEquals(c, channelName)),
                nameof(ChannelType.Graph) => Channels.Graphs.FirstOrDefault(c => ChannelEquals(c, channelName)),
                nameof(ChannelType.Onboard) => Channels.Onboards.FirstOrDefault(c => ChannelEquals(c, channelName)),
                _ => null,
            };
        }

    }
}