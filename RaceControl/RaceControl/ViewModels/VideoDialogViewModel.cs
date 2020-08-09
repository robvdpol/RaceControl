using LibVLCSharp.Shared;
using LibVLCSharp.Shared.Structures;
using Prism.Commands;
using Prism.Events;
using Prism.Services.Dialogs;
using RaceControl.Common;
using RaceControl.Core.Mvvm;
using RaceControl.Events;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RaceControl.ViewModels
{
    public class VideoDialogViewModel : DialogViewModelBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly LibVLC _libVLC;
        private readonly Timer _showControlsTimer = new Timer(2000) { AutoReset = false };

        private ICommand _mouseEnterCommand;
        private ICommand _mouseLeaveCommand;
        private ICommand _mouseMoveCommand;
        private ICommand _mouseDownCommand;
        private ICommand _togglePauseCommand;
        private ICommand _toggleMuteCommand;
        private ICommand _fastForwardCommand;
        private ICommand _syncSessionCommand;
        private ICommand _toggleFullScreenCommand;
        private ICommand _audioTrackSelectionChangedCommand;
        private ICommand _scanChromecastCommand;
        private ICommand _castVideoCommand;

        private Guid _uniqueIdentifier = Guid.NewGuid();
        private Process _streamlinkProcess;
        private Func<string, Task<string>> _contentUrlFunc;
        private string _contentUrl;
        private string _syncUID;
        private string _title;
        private bool _isLive;
        private MediaPlayer _mediaPlayer;
        private Media _media;
        private ObservableCollection<TrackDescription> _audioTrackDescriptions;
        private long _duration;
        private long _sliderTime;
        private TimeSpan _displayTime;
        private RendererDiscoverer _rendererDiscoverer;
        private ObservableCollection<RendererItem> _rendererItems;
        private RendererItem _selectedRendererItem;
        private bool _showControls;
        private bool _fullScreen;
        private WindowStyle _windowStyle = WindowStyle.SingleBorderWindow;
        private ResizeMode _resizeMode = ResizeMode.CanResize;
        private WindowState _windowState = WindowState.Normal;

        public VideoDialogViewModel(IEventAggregator eventAggregator, LibVLC libVLC)
        {
            _eventAggregator = eventAggregator;
            _libVLC = libVLC;
        }

        public ICommand MouseEnterCommand => _mouseEnterCommand ??= new DelegateCommand(MouseEnterOrLeaveOrMoveExecute);
        public ICommand MouseLeaveCommand => _mouseLeaveCommand ??= new DelegateCommand(MouseEnterOrLeaveOrMoveExecute);
        public ICommand MouseMoveCommand => _mouseMoveCommand ??= new DelegateCommand(MouseEnterOrLeaveOrMoveExecute);
        public ICommand MouseDownCommand => _mouseDownCommand ??= new DelegateCommand<MouseButtonEventArgs>(MouseDownExecute);
        public ICommand TogglePauseCommand => _togglePauseCommand ??= new DelegateCommand(TogglePauseExecute);
        public ICommand ToggleMuteCommand => _toggleMuteCommand ??= new DelegateCommand(ToggleMuteExecute);
        public ICommand FastForwardCommand => _fastForwardCommand ??= new DelegateCommand<string>(FastForwardExecute, CanFastForwardExecute).ObservesProperty(() => IsLive);
        public ICommand SyncSessionCommand => _syncSessionCommand ??= new DelegateCommand(SyncSessionExecute, CanSyncSessionExecute).ObservesProperty(() => IsLive);
        public ICommand ToggleFullScreenCommand => _toggleFullScreenCommand ??= new DelegateCommand(ToggleFullScreenExecute);
        public ICommand AudioTrackSelectionChangedCommand => _audioTrackSelectionChangedCommand ??= new DelegateCommand<SelectionChangedEventArgs>(AudioTrackSelectionChangedExecute);
        public ICommand ScanChromecastCommand => _scanChromecastCommand ??= new DelegateCommand(ScanChromecastExecute, CanScanChromecastExecute).ObservesProperty(() => RendererDiscoverer);
        public ICommand CastVideoCommand => _castVideoCommand ??= new DelegateCommand(CastVideoExecute, CanCastVideoExecute).ObservesProperty(() => SelectedRendererItem);

        public Func<string, Task<string>> ContentUrlFunc
        {
            get => _contentUrlFunc;
            set => SetProperty(ref _contentUrlFunc, value);
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

        public override string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public bool IsLive
        {
            get => _isLive;
            set => SetProperty(ref _isLive, value);
        }

        public MediaPlayer MediaPlayer
        {
            get => _mediaPlayer;
            set => SetProperty(ref _mediaPlayer, value);
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

        public bool FullScreen
        {
            get => _fullScreen;
            set => SetProperty(ref _fullScreen, value);
        }

        public WindowStyle WindowStyle
        {
            get => _windowStyle;
            set => SetProperty(ref _windowStyle, value);
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

        public override async void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);

            ContentUrlFunc = parameters.GetValue<Func<string, Task<string>>>(ParameterNames.ContentUrlFunc);
            ContentUrl = parameters.GetValue<string>(ParameterNames.ContentUrl);
            SyncUID = parameters.GetValue<string>(ParameterNames.SyncUID);
            Title = parameters.GetValue<string>(ParameterNames.Title);
            IsLive = parameters.GetValue<bool>(ParameterNames.IsLive);

            var streamUrl = await ContentUrlFunc.Invoke(ContentUrl);

            if (IsLive)
            {
                _streamlinkProcess = StartStreamlink(streamUrl, out streamUrl);
            }

            _media = CreateMedia(streamUrl);

            MediaPlayer = CreateMediaPlayer();
            MediaPlayer.Play(_media);

            _showControlsTimer.Elapsed += ShowControlsTimer_Elapsed;
            _showControlsTimer.Start();

            _eventAggregator.GetEvent<SyncStreamsEvent>().Subscribe(OnSyncSession);
        }

        public override void OnDialogClosed()
        {
            base.OnDialogClosed();

            _eventAggregator.GetEvent<SyncStreamsEvent>().Unsubscribe(OnSyncSession);

            _showControlsTimer.Stop();
            _showControlsTimer.Elapsed -= ShowControlsTimer_Elapsed;
            _showControlsTimer.Dispose();

            RemoveMedia(_media);
            RemoveMediaPlayer(MediaPlayer);

            if (RendererDiscoverer != null)
            {
                RendererDiscoverer.Stop();
                RendererDiscoverer.ItemAdded -= RendererDiscoverer_ItemAdded;
                RendererDiscoverer.Dispose();
            }

            if (_streamlinkProcess != null)
            {
                _streamlinkProcess.Kill(true);
            }
        }

        private void Media_DurationChanged(object sender, MediaDurationChangedEventArgs e)
        {
            Duration = e.Duration;
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
            ShowControls = false;
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

        private void MouseEnterOrLeaveOrMoveExecute()
        {
            _showControlsTimer.Stop();
            ShowControls = true;
            _showControlsTimer.Start();
        }

        private void MouseDownExecute(MouseButtonEventArgs args)
        {
            if (args.ClickCount == 2 && ToggleFullScreenCommand.CanExecute(null))
            {
                ToggleFullScreenCommand.Execute(null);
            }
        }

        private void TogglePauseExecute()
        {
            if (MediaPlayer.CanPause)
            {
                MediaPlayer.Pause();
            }
        }

        private void ToggleMuteExecute()
        {
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
                var time = MediaPlayer.Time + (seconds * 1000);
                SetMediaPlayerTime(time);
            }
        }

        private bool CanSyncSessionExecute()
        {
            return !IsLive;
        }

        private void SyncSessionExecute()
        {
            if (MediaPlayer.IsPlaying)
            {
                var payload = new SyncStreamsEventPayload(_uniqueIdentifier, SyncUID, MediaPlayer.Time);
                _eventAggregator.GetEvent<SyncStreamsEvent>().Publish(payload);
            }
        }

        private void OnSyncSession(SyncStreamsEventPayload payload)
        {
            if (SyncUID == payload.SyncUID && _uniqueIdentifier != payload.RequesterIdentifier)
            {
                SetMediaPlayerTime(payload.Time, true);
            }
        }

        private void ToggleFullScreenExecute()
        {
            if (!FullScreen)
            {
                SetFullScreen();
            }
            else
            {
                SetWindowed();
            }
        }

        private void AudioTrackSelectionChangedExecute(SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0)
            {
                var trackDescription = (TrackDescription)args.AddedItems[0];
                MediaPlayer.SetAudioTrack(trackDescription.Id);
            }
        }

        private bool CanScanChromecastExecute()
        {
            return RendererDiscoverer == null;
        }

        private void ScanChromecastExecute()
        {
            RendererDiscoverer = new RendererDiscoverer(_libVLC);
            RendererDiscoverer.ItemAdded += RendererDiscoverer_ItemAdded;
            RendererDiscoverer.Start();
        }

        private bool CanCastVideoExecute()
        {
            return SelectedRendererItem != null;
        }

        private async void CastVideoExecute()
        {
            var time = MediaPlayer.Time;
            MediaPlayer.Stop();

            if (!IsLive)
            {
                RemoveMedia(_media);
                var streamUrl = await ContentUrlFunc.Invoke(ContentUrl);
                _media = CreateMedia(streamUrl);
            }

            MediaPlayer.SetRenderer(SelectedRendererItem);
            MediaPlayer.Play(_media);

            if (!IsLive)
            {
                SetMediaPlayerTime(time);
            }
        }

        private void SetWindowed()
        {
            FullScreen = false;
            WindowStyle = WindowStyle.SingleBorderWindow;
            ResizeMode = ResizeMode.CanResize;
            WindowState = WindowState.Normal;
        }

        private void SetFullScreen()
        {
            FullScreen = true;
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            WindowState = WindowState.Maximized;
        }

        private void SetMediaPlayerTime(long time, bool mustBePlaying = false)
        {
            if (!mustBePlaying || MediaPlayer.IsPlaying)
            {
                MediaPlayer.Time = time;
            }
        }

        private Media CreateMedia(string url)
        {
            var media = new Media(_libVLC, url, FromType.FromLocation);
            media.DurationChanged += Media_DurationChanged;

            return media;
        }

        private void RemoveMedia(Media media)
        {
            media.DurationChanged -= Media_DurationChanged;
            media.Dispose();
        }

        private MediaPlayer CreateMediaPlayer()
        {
            var mediaPlayer = new MediaPlayer(_libVLC)
            {
                EnableHardwareDecoding = true,
                EnableMouseInput = false,
                EnableKeyInput = false,
                FileCaching = 1000,
                NetworkCaching = 2000
            };

            mediaPlayer.TimeChanged += MediaPlayer_TimeChanged;
            mediaPlayer.ESAdded += MediaPlayer_ESAdded;
            mediaPlayer.ESDeleted += MediaPlayer_ESDeleted;

            return mediaPlayer;
        }

        private void RemoveMediaPlayer(MediaPlayer mediaPlayer)
        {
            mediaPlayer.Stop();
            mediaPlayer.TimeChanged -= MediaPlayer_TimeChanged;
            mediaPlayer.ESAdded -= MediaPlayer_ESAdded;
            mediaPlayer.ESDeleted -= MediaPlayer_ESDeleted;
            mediaPlayer.Dispose();
        }

        private static Process StartStreamlink(string streamUrl, out string streamlinkUrl)
        {
            var port = SocketUtils.GetFreePort();
            streamlinkUrl = $"http://127.0.0.1:{port}";

            return Process.Start(new ProcessStartInfo
            {
                FileName = @".\streamlink\streamlink.bat",
                Arguments = $"\"{streamUrl}\" best --player-external-http --player-external-http-port {port} --hls-audio-select *",
                UseShellExecute = false,
                CreateNoWindow = true
            });
        }
    }
}