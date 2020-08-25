using LibVLCSharp.Shared;
using LibVLCSharp.Shared.Structures;
using NLog;
using Prism.Commands;
using Prism.Events;
using Prism.Services.Dialogs;
using RaceControl.Common.Settings;
using RaceControl.Core.Mvvm;
using RaceControl.Enums;
using RaceControl.Events;
using RaceControl.Interfaces;
using RaceControl.Services.Interfaces.F1TV;
using RaceControl.Streamlink;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
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
        private readonly ILogger _logger;
        private readonly IEventAggregator _eventAggregator;
        private readonly IApiService _apiService;
        private readonly IStreamlinkLauncher _streamlinkLauncher;
        private readonly LibVLC _libVLC;

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

        private string _token;
        private string _name;
        private ContentType _contentType;
        private string _contentUrl;
        private string _syncUID;
        private bool _isLive;
        private bool _isPaused;
        private bool _isMuted;
        private bool _isRecording;
        private bool _isCasting;
        private Process _streamlinkProcess;
        private Process _streamlinkRecordingProcess;
        private Media _media;
        private ObservableCollection<TrackDescription> _audioTrackDescriptions;
        private long _duration;
        private long _sliderTime;
        private TimeSpan _displayTime;
        private RendererDiscoverer _rendererDiscoverer;
        private ObservableCollection<RendererItem> _rendererItems;
        private RendererItem _selectedRendererItem;
        private Timer _showControlsTimer;
        private bool _showControls;
        private double _top;
        private double _left;
        private double _width = 1200;
        private double _height = 705;
        private ResizeMode _resizeMode = ResizeMode.CanResize;
        private WindowState _windowState = WindowState.Normal;
        private WindowStartupLocation _startupLocation = WindowStartupLocation.CenterOwner;

        public VideoDialogViewModel(
            ILogger logger,
            IEventAggregator eventAggregator,
            IApiService apiService,
            IStreamlinkLauncher streamlinkLauncher,
            LibVLC libVLC,
            MediaPlayer mediaPlayer)
        {
            _logger = logger;
            _eventAggregator = eventAggregator;
            _apiService = apiService;
            _streamlinkLauncher = streamlinkLauncher;
            _libVLC = libVLC;
            MediaPlayer = mediaPlayer;
        }

        public ICommand MouseDownVideoCommand => _mouseDownVideoCommand ??= new DelegateCommand<MouseButtonEventArgs>(MouseDownVideoExecute);
        public ICommand MouseMoveVideoCommand => _mouseMoveVideoCommand ??= new DelegateCommand(MouseEnterOrLeaveOrMoveVideoExecute);
        public ICommand MouseEnterVideoCommand => _mouseEnterVideoCommand ??= new DelegateCommand(MouseEnterOrLeaveOrMoveVideoExecute);
        public ICommand MouseLeaveVideoCommand => _mouseLeaveVideoCommand ??= new DelegateCommand(MouseEnterOrLeaveOrMoveVideoExecute);
        public ICommand MouseMoveControlBarCommand => _mouseMoveControlBarCommand ??= new DelegateCommand(MouseMoveControlBarExecute);
        public ICommand MouseEnterControlBarCommand => _mouseEnterControlBarCommand ??= new DelegateCommand(MouseEnterControlBarExecute);
        public ICommand MouseLeaveControlBarCommand => _mouseLeaveControlBarCommand ??= new DelegateCommand(MouseLeaveControlBarExecute);
        public ICommand TogglePauseCommand => _togglePauseCommand ??= new DelegateCommand(TogglePauseExecute);
        public ICommand ToggleMuteCommand => _toggleMuteCommand ??= new DelegateCommand(ToggleMuteExecute);
        public ICommand FastForwardCommand => _fastForwardCommand ??= new DelegateCommand<string>(FastForwardExecute, CanFastForwardExecute).ObservesProperty(() => IsLive);
        public ICommand SyncSessionCommand => _syncSessionCommand ??= new DelegateCommand(SyncSessionExecute, CanSyncSessionExecute).ObservesProperty(() => IsLive);
        public ICommand ToggleRecordingCommand => _toggleRecordingCommand ??= new DelegateCommand(ToggleRecordingExecute, CanToggleRecordingExecute).ObservesProperty(() => IsLive).ObservesProperty(() => IsRecording).ObservesProperty(() => IsPaused);
        public ICommand ToggleFullScreenCommand => _toggleFullScreenCommand ??= new DelegateCommand(ToggleFullScreenExecute);
        public ICommand MoveToCornerCommand => _moveToCornerCommand ??= new DelegateCommand<WindowLocation?>(MoveToCornerExecute, CanMoveToCornerExecute).ObservesProperty(() => WindowState);
        public ICommand AudioTrackSelectionChangedCommand => _audioTrackSelectionChangedCommand ??= new DelegateCommand<SelectionChangedEventArgs>(AudioTrackSelectionChangedExecute);
        public ICommand ScanChromecastCommand => _scanChromecastCommand ??= new DelegateCommand(ScanChromecastExecute, CanScanChromecastExecute).ObservesProperty(() => RendererDiscoverer);
        public ICommand StartCastVideoCommand => _startCastVideoCommand ??= new DelegateCommand(StartCastVideoExecute, CanStartCastVideoExecute).ObservesProperty(() => SelectedRendererItem);
        public ICommand StopCastVideoCommand => _stopCastVideoCommand ??= new DelegateCommand(StopCastVideoExecute, CanStopCastVideoExecute).ObservesProperty(() => IsCasting);

        public Guid UniqueIdentifier { get; } = Guid.NewGuid();

        public MediaPlayer MediaPlayer { get; }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public ContentType ContentType
        {
            get => _contentType;
            set => SetProperty(ref _contentType, value);
        }

        public string ContentUrl
        {
            get => _contentUrl;
            set => SetProperty(ref _contentUrl, value);
        }

        public string SyncUID
        {
            get => _syncUID;
            set => SetProperty(ref _syncUID, value);
        }

        public bool IsLive
        {
            get => _isLive;
            set => SetProperty(ref _isLive, value);
        }

        public bool IsPaused
        {
            get => _isPaused;
            set => SetProperty(ref _isPaused, value);
        }

        public bool IsMuted
        {
            get => _isMuted;
            set => SetProperty(ref _isMuted, value);
        }

        public bool IsRecording
        {
            get => _isRecording;
            set => SetProperty(ref _isRecording, value);
        }

        public bool IsCasting
        {
            get => _isCasting;
            set => SetProperty(ref _isCasting, value);
        }

        public Media Media
        {
            get => _media;
            set => SetProperty(ref _media, value);
        }

        public ObservableCollection<TrackDescription> AudioTrackDescriptions
        {
            get => _audioTrackDescriptions ??= new ObservableCollection<TrackDescription>();
            set => SetProperty(ref _audioTrackDescriptions, value);
        }

        public long Duration
        {
            get => _duration;
            set => SetProperty(ref _duration, value);
        }

        public long SliderTime
        {
            get => _sliderTime;
            set
            {
                if (SetProperty(ref _sliderTime, value))
                {
                    SetMediaPlayerTime(_sliderTime);
                }
            }
        }

        public TimeSpan DisplayTime
        {
            get => _displayTime;
            set => SetProperty(ref _displayTime, value);
        }

        public RendererDiscoverer RendererDiscoverer
        {
            get => _rendererDiscoverer;
            set => SetProperty(ref _rendererDiscoverer, value);
        }

        public ObservableCollection<RendererItem> RendererItems
        {
            get => _rendererItems ??= new ObservableCollection<RendererItem>();
            set => SetProperty(ref _rendererItems, value);
        }

        public RendererItem SelectedRendererItem
        {
            get => _selectedRendererItem;
            set => SetProperty(ref _selectedRendererItem, value);
        }

        public bool ShowControls
        {
            get => _showControls;
            set => SetProperty(ref _showControls, value);
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
                ChannelName = Name
            };
        }

        public override async void OnDialogOpened(IDialogParameters parameters)
        {
            _token = parameters.GetValue<string>(ParameterNames.TOKEN);
            Title = parameters.GetValue<string>(ParameterNames.TITLE);
            Name = parameters.GetValue<string>(ParameterNames.NAME);
            ContentType = parameters.GetValue<ContentType>(ParameterNames.CONTENT_TYPE);
            ContentUrl = parameters.GetValue<string>(ParameterNames.CONTENT_URL);
            SyncUID = parameters.GetValue<string>(ParameterNames.SYNC_UID);
            IsLive = parameters.GetValue<bool>(ParameterNames.IS_LIVE);

            var instance = parameters.GetValue<VideoDialogInstance>(ParameterNames.INSTANCE);

            if (instance != null)
            {
                StartupLocation = WindowStartupLocation.Manual;
                ResizeMode = (ResizeMode)instance.ResizeMode;
                WindowState = (WindowState)instance.WindowState;
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

            var streamUrl = await GenerateStreamUrlAsync();

            if (IsLive)
            {
                _streamlinkProcess = _streamlinkLauncher.StartStreamlinkExternal(streamUrl, out streamUrl);
            }

            CreateMediaPlayer();
            CreateMedia(streamUrl);
            StartPlayback();

            _showControlsTimer = new Timer(2000) { AutoReset = false };
            _showControlsTimer.Elapsed += ShowControlsTimer_Elapsed;

            _eventAggregator.GetEvent<SyncStreamsEvent>().Subscribe(OnSyncSession);
        }

        public override void OnDialogClosed()
        {
            _eventAggregator.GetEvent<SyncStreamsEvent>().Unsubscribe(OnSyncSession);

            if (_showControlsTimer != null)
            {
                _showControlsTimer.Stop();
                _showControlsTimer.Elapsed -= ShowControlsTimer_Elapsed;
                _showControlsTimer.Dispose();
                _showControlsTimer = null;
            }

            StopPlayback();
            RemoveMedia();
            RemoveMediaPlayer();

            if (RendererDiscoverer != null)
            {
                RendererDiscoverer.Stop();
                RendererDiscoverer.ItemAdded -= RendererDiscoverer_ItemAdded;
                RendererDiscoverer.Dispose();
            }

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

        private void Media_DurationChanged(object sender, MediaDurationChangedEventArgs e)
        {
            Duration = e.Duration;
        }

        private void MediaPlayer_Playing(object sender, EventArgs e)
        {
            // Prevent closing the dialog too soon, this causes problems with LibVLC
            Opened = true;
            IsPaused = false;
        }

        private void MediaPlayer_Paused(object sender, EventArgs e)
        {
            IsPaused = true;
        }

        private void MediaPlayer_Unmuted(object sender, EventArgs e)
        {
            IsMuted = false;
        }

        private void MediaPlayer_Muted(object sender, EventArgs e)
        {
            IsMuted = true;
        }

        private void MediaPlayer_TimeChanged(object sender, MediaPlayerTimeChangedEventArgs e)
        {
            SetProperty(ref _sliderTime, e.Time, nameof(SliderTime));
            DisplayTime = TimeSpan.FromMilliseconds(e.Time);
        }

        private void MediaPlayer_ESAdded(object sender, MediaPlayerESAddedEventArgs e)
        {
            if (e.Id >= 0)
            {
                switch (e.Type)
                {
                    case TrackType.Audio:
                        var audioTrackDescription = MediaPlayer.AudioTrackDescription.First(p => p.Id == e.Id);

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            AudioTrackDescriptions.Add(audioTrackDescription);
                        });
                        break;
                }
            }
        }

        private void MediaPlayer_ESDeleted(object sender, MediaPlayerESDeletedEventArgs e)
        {
            if (e.Id >= 0)
            {
                switch (e.Type)
                {
                    case TrackType.Audio:
                        var audioTrackDescription = AudioTrackDescriptions.First(p => p.Id == e.Id);

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            AudioTrackDescriptions.Remove(audioTrackDescription);
                        });
                        break;
                }
            }
        }

        private void ShowControlsTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ShowControls = false;
                Mouse.OverrideCursor = Cursors.None;
            });
        }

        private void RendererDiscoverer_ItemAdded(object sender, RendererDiscovererItemAddedEventArgs e)
        {
            if (e.RendererItem.CanRenderVideo)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    RendererItems.Add(e.RendererItem);
                });
            }
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
            if (MediaPlayer.CanPause)
            {
                _logger.Info("Toggling pause...");
                MediaPlayer.Pause();
            }
        }

        private void ToggleMuteExecute()
        {
            _logger.Info("Toggling mute...");
            MediaPlayer.ToggleMute();
        }

        private bool CanFastForwardExecute(string arg)
        {
            return !IsLive;
        }

        private void FastForwardExecute(string value)
        {
            if (int.TryParse(value, out var seconds))
            {
                _logger.Info($"Fast forwarding stream {seconds} seconds...");
                var time = MediaPlayer.Time + seconds * 1000;
                SetMediaPlayerTime(time);
            }
        }

        private bool CanSyncSessionExecute()
        {
            return !IsLive;
        }

        private void SyncSessionExecute()
        {
            var payload = new SyncStreamsEventPayload(UniqueIdentifier, SyncUID, MediaPlayer.Time);
            _logger.Info($"Syncing streams with sync-UID '{payload.SyncUID}' to timestamp '{payload.Time}'...");
            _eventAggregator.GetEvent<SyncStreamsEvent>().Publish(payload);
        }

        private bool CanToggleRecordingExecute()
        {
            return IsLive && (IsRecording || !IsPaused);
        }

        private async void ToggleRecordingExecute()
        {
            if (!IsRecording)
            {
                await StartRecording();
            }
            else
            {
                StopRecording();
            }

            IsRecording = !IsRecording;
        }

        private void OnSyncSession(SyncStreamsEventPayload payload)
        {
            if (UniqueIdentifier != payload.RequesterIdentifier && SyncUID == payload.SyncUID)
            {
                SetMediaPlayerTime(payload.Time);
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
            _logger.Info($"Moving window to corner '{location}'...");

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

            _logger.Info($"Done moving window (top: {Top}, left: {Left}, width: {Width}, height: {Height}).");
        }

        private void AudioTrackSelectionChangedExecute(SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is TrackDescription trackDescription)
            {
                _logger.Info($"Changing audio track to '{trackDescription.Id}'...");
                MediaPlayer.SetAudioTrack(trackDescription.Id);
                _logger.Info("Done changing audio track.");
            }
        }

        private bool CanScanChromecastExecute()
        {
            return RendererDiscoverer == null;
        }

        private void ScanChromecastExecute()
        {
            _logger.Info("Scanning for Chromecast devices...");
            RendererDiscoverer = new RendererDiscoverer(_libVLC);
            RendererDiscoverer.ItemAdded += RendererDiscoverer_ItemAdded;
            RendererDiscoverer.Start();
        }

        private bool CanStartCastVideoExecute()
        {
            return SelectedRendererItem != null;
        }

        private async void StartCastVideoExecute()
        {
            _logger.Info($"Starting casting of video with renderer '{SelectedRendererItem.Name}'...");
            await ChangeRendererAsync(SelectedRendererItem);
            IsCasting = true;
        }

        private bool CanStopCastVideoExecute()
        {
            return IsCasting;
        }

        private async void StopCastVideoExecute()
        {
            _logger.Info("Stopping casting of video...");
            await ChangeRendererAsync(null);
            IsCasting = false;
        }

        private async Task<string> GenerateStreamUrlAsync()
        {
            _logger.Info($"Getting tokenised URL for content-type '{ContentType}' and content-url '{ContentUrl}'...");

            return await _apiService.GetTokenisedUrlAsync(_token, ContentType, ContentUrl);
        }

        private async Task ChangeRendererAsync(RendererItem renderer)
        {
            _logger.Info($"Changing renderer to '{renderer?.Name}'...");
            var streamTime = MediaPlayer.Time;
            var streamUrl = IsLive ? Media.Mrl : await GenerateStreamUrlAsync();

            StopPlayback();
            RemoveMedia();
            CreateMedia(streamUrl);
            StartPlayback(renderer);

            if (!IsLive)
            {
                SetMediaPlayerTime(streamTime);
            }

            _logger.Info("Done changing renderer.");
        }

        private async Task StartRecording()
        {
            _logger.Info("Starting recording process...");
            var streamUrl = await GenerateStreamUrlAsync();
            _streamlinkRecordingProcess = _streamlinkLauncher.StartStreamlinkRecording(streamUrl, Title);
            _logger.Info("Recording process started.");
        }

        private void StopRecording()
        {
            _logger.Info("Stopping recording process...");
            CleanupProcess(_streamlinkRecordingProcess);
            _streamlinkRecordingProcess = null;
            _logger.Info("Recording process stopped.");
        }

        private void SetFullScreen()
        {
            _logger.Info("Changing to fullscreen mode...");
            ResizeMode = ResizeMode.NoResize;
            WindowState = WindowState.Maximized;
        }

        private void SetWindowed()
        {
            _logger.Info("Changing to windowed mode...");
            WindowState = WindowState.Normal;
        }

        private void SetMediaPlayerTime(long time)
        {
            _logger.Info($"Setting mediaplayer time to '{time}'...");
            MediaPlayer.Time = time;
        }

        private void CreateMediaPlayer()
        {
            _logger.Info("Creating mediaplayer...");
            MediaPlayer.Playing += MediaPlayer_Playing;
            MediaPlayer.Paused += MediaPlayer_Paused;
            MediaPlayer.Muted += MediaPlayer_Muted;
            MediaPlayer.Unmuted += MediaPlayer_Unmuted;
            MediaPlayer.TimeChanged += MediaPlayer_TimeChanged;
            MediaPlayer.ESAdded += MediaPlayer_ESAdded;
            MediaPlayer.ESDeleted += MediaPlayer_ESDeleted;
            _logger.Info("Done creating mediaplayer.");
        }

        private void CreateMedia(string url)
        {
            _logger.Info("Creating media...");
            Media = new Media(_libVLC, url, FromType.FromLocation);
            Media.DurationChanged += Media_DurationChanged;
            _logger.Info("Done creating media.");
        }

        private void StartPlayback(RendererItem renderer = null)
        {
            _logger.Info("Starting playback...");
            AudioTrackDescriptions.Clear();
            MediaPlayer.SetRenderer(renderer);
            MediaPlayer.Play(Media);
            _logger.Info("Done starting playback.");
        }

        private void RemoveMediaPlayer()
        {
            _logger.Info("Removing mediaplayer...");
            MediaPlayer.Playing -= MediaPlayer_Playing;
            MediaPlayer.Paused -= MediaPlayer_Paused;
            MediaPlayer.Muted -= MediaPlayer_Muted;
            MediaPlayer.Unmuted -= MediaPlayer_Unmuted;
            MediaPlayer.TimeChanged -= MediaPlayer_TimeChanged;
            MediaPlayer.ESAdded -= MediaPlayer_ESAdded;
            MediaPlayer.ESDeleted -= MediaPlayer_ESDeleted;
            MediaPlayer.Dispose();
            _logger.Info("Done removing mediaplayer.");
        }

        private void RemoveMedia()
        {
            _logger.Info("Removing media...");
            Media.DurationChanged -= Media_DurationChanged;
            Media.Dispose();
            _logger.Info("Done removing media.");
        }

        private void StopPlayback()
        {
            _logger.Info("Stopping playback...");
            MediaPlayer.Stop();
            AudioTrackDescriptions.Clear();
            _logger.Info("Done stopping playback.");
        }
    }
}