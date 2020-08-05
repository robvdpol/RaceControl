using LibVLCSharp.Shared;
using LibVLCSharp.Shared.Structures;
using Prism.Commands;
using Prism.Events;
using Prism.Services.Dialogs;
using RaceControl.Core.Mvvm;
using RaceControl.Events;
using RaceControl.Services.Interfaces.F1TV;
using RaceControl.Services.Interfaces.F1TV.Api;
using System;
using System.Collections.ObjectModel;
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
        private readonly IApiService _apiService;
        private readonly LibVLC _libVLC;
        private readonly Timer _showControlsTimer = new Timer(2000) { AutoReset = false };

        private ICommand _mouseEnterCommand;
        private ICommand _mouseLeaveCommand;
        private ICommand _mouseDownCommand;
        private ICommand _togglePauseCommand;
        private ICommand _toggleMuteCommand;
        private ICommand _fastForwardCommand;
        private ICommand _syncSessionCommand;
        private ICommand _toggleFullScreenCommand;
        private ICommand _audioTrackSelectionChangedCommand;
        private ICommand _castVideoCommand;

        private string _token;
        private Session _session;
        private Channel _channel;
        private Episode _episode;
        private SubscriptionToken _syncSessionSubscriptionToken;
        private MediaPlayer _mediaPlayer;
        private MediaPlayer _mediaPlayerCast;
        private TimeSpan _time;
        private ObservableCollection<TrackDescription> _audioTrackDescriptions;
        private RendererDiscoverer _rendererDiscoverer;
        private ObservableCollection<RendererItem> _rendererItems;
        private RendererItem _selectedRendererItem;
        private bool _showControls;
        private bool _fullScreen;
        private WindowStyle _windowStyle;
        private ResizeMode _resizeMode;
        private WindowState _windowState;

        public VideoDialogViewModel(IEventAggregator eventAggregator, IApiService apiService, LibVLC libVLC)
        {
            _eventAggregator = eventAggregator;
            _apiService = apiService;
            _libVLC = libVLC;
        }

        public override string Title => _channel != null ? $"{_session} - {_channel}" : $"{_episode}";

        public ICommand MouseEnterCommand => _mouseEnterCommand ??= new DelegateCommand(MouseEnterExecute);
        public ICommand MouseLeaveCommand => _mouseLeaveCommand ??= new DelegateCommand(MouseLeaveExecute);
        public ICommand MouseDownCommand => _mouseDownCommand ??= new DelegateCommand<MouseButtonEventArgs>(MouseDownExecute);
        public ICommand TogglePauseCommand => _togglePauseCommand ??= new DelegateCommand(TogglePauseExecute);
        public ICommand ToggleMuteCommand => _toggleMuteCommand ??= new DelegateCommand(ToggleMuteExecute);
        public ICommand FastForwardCommand => _fastForwardCommand ??= new DelegateCommand<string>(FastForwardExecute);
        public ICommand SyncSessionCommand => _syncSessionCommand ??= new DelegateCommand(SyncSessionExecute);
        public ICommand ToggleFullScreenCommand => _toggleFullScreenCommand ??= new DelegateCommand(ToggleFullScreenExecute);
        public ICommand AudioTrackSelectionChangedCommand => _audioTrackSelectionChangedCommand ??= new DelegateCommand<SelectionChangedEventArgs>(AudioTrackSelectionChangedExecute);
        public ICommand CastVideoCommand => _castVideoCommand ??= new DelegateCommand(CastVideoExecute, CanCastVideoExecute).ObservesProperty(() => SelectedRendererItem);

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

        public TimeSpan Time
        {
            get => _time;
            set => SetProperty(ref _time, value);
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

            SetWindowed();

            _token = parameters.GetValue<string>("token");
            _session = parameters.GetValue<Session>("session");
            _channel = parameters.GetValue<Channel>("channel");
            _episode = parameters.GetValue<Episode>("episode");

            MediaPlayer = CreateMediaPlayer();
            MediaPlayer.ESAdded += MediaPlayer_ESAdded;
            MediaPlayer.ESDeleted += MediaPlayer_ESDeleted;
            MediaPlayer.TimeChanged += MediaPlayer_TimeChanged;

            if (MediaPlayer.Play(await CreatePlaybackMedia()))
            {
                _syncSessionSubscriptionToken = _eventAggregator.GetEvent<SyncSessionEvent>().Subscribe(OnSyncSession);
            }

            _rendererDiscoverer = new RendererDiscoverer(_libVLC);
            _rendererDiscoverer.ItemAdded += RendererDiscoverer_ItemAdded;
            _rendererDiscoverer.Start();

            _showControlsTimer.Elapsed += ShowControlsTimer_Elapsed;
        }

        public override void OnDialogClosed()
        {
            base.OnDialogClosed();

            _showControlsTimer.Elapsed -= ShowControlsTimer_Elapsed;
            _showControlsTimer.Stop();
            _showControlsTimer.Dispose();

            _rendererDiscoverer.ItemAdded -= RendererDiscoverer_ItemAdded;
            _rendererDiscoverer.Stop();
            _rendererDiscoverer.Dispose();

            if (_syncSessionSubscriptionToken != null)
            {
                _eventAggregator.GetEvent<SyncSessionEvent>().Unsubscribe(_syncSessionSubscriptionToken);
            }

            MediaPlayer.ESAdded -= MediaPlayer_ESAdded;
            MediaPlayer.ESDeleted -= MediaPlayer_ESDeleted;
            MediaPlayer.TimeChanged -= MediaPlayer_TimeChanged;
            MediaPlayer.Stop();
            MediaPlayer.Dispose();

            if (_mediaPlayerCast != null)
            {
                _mediaPlayerCast.Stop();
                _mediaPlayerCast.Dispose();
            }
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

        private void MediaPlayer_TimeChanged(object sender, MediaPlayerTimeChangedEventArgs e)
        {
            Time = TimeSpan.FromMilliseconds(e.Time);
        }

        private void ShowControlsTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ShowControls = false;
        }

        private void MouseEnterExecute()
        {
            _showControlsTimer.Stop();
            ShowControls = true;
        }

        private void MouseLeaveExecute()
        {
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

            if (_mediaPlayerCast != null && _mediaPlayerCast.CanPause)
            {
                _mediaPlayerCast.Pause();
            }
        }

        private void ToggleMuteExecute()
        {
            MediaPlayer.ToggleMute();
        }

        private void FastForwardExecute(string value)
        {
            if (int.TryParse(value, out var seconds))
            {
                var time = MediaPlayer.Time + (seconds * 1000);
                SetMediaPlayerTime(time, false, false);
            }
        }

        private void SyncSessionExecute()
        {
            if (MediaPlayer.IsPlaying && _session != null)
            {
                var payload = new SyncSessionEventPayload(_session.UID, MediaPlayer.Time);
                _eventAggregator.GetEvent<SyncSessionEvent>().Publish(payload);
            }
        }

        private void OnSyncSession(SyncSessionEventPayload payload)
        {
            if (_session?.UID == payload.SessionUID)
            {
                SetMediaPlayerTime(payload.Time, true, false);
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
            var trackDescription = (TrackDescription)args.AddedItems[0];
            MediaPlayer.SetAudioTrack(trackDescription.Id);
        }

        private bool CanCastVideoExecute()
        {
            return SelectedRendererItem != null;
        }

        private async void CastVideoExecute()
        {
            _mediaPlayerCast ??= CreateMediaPlayer();
            _mediaPlayerCast.Stop();
            _mediaPlayerCast.SetRenderer(SelectedRendererItem);

            var media = await CreatePlaybackMedia();

            if (_mediaPlayerCast.Play(media))
            {
                SetMediaPlayerTime(MediaPlayer.Time, false, true);
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

        private void SetMediaPlayerTime(long time, bool mustBePlaying, bool castOnly)
        {
            if (!castOnly && (!mustBePlaying || MediaPlayer.IsPlaying))
            {
                MediaPlayer.Time = time;
            }

            if (_mediaPlayerCast == null)
            {
                return;
            }

            if (!mustBePlaying || _mediaPlayerCast.IsPlaying)
            {
                _mediaPlayerCast.Time = time;
            }
        }

        private MediaPlayer CreateMediaPlayer()
        {
            return new MediaPlayer(_libVLC)
            {
                EnableHardwareDecoding = true,
                EnableMouseInput = false,
                EnableKeyInput = false
            };
        }

        private async Task<Media> CreatePlaybackMedia()
        {
            string url;

            if (_channel != null)
            {
                url = await _apiService.GetTokenisedUrlForChannelAsync(_token, _channel.Self);
            }
            else
            {
                url = await _apiService.GetTokenisedUrlForAssetAsync(_token, _episode.Items.First());
            }

            return new Media(_libVLC, url, FromType.FromLocation);
        }
    }
}