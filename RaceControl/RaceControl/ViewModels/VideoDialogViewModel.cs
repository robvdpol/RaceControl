using LibVLCSharp.Shared;
using LibVLCSharp.Shared.Structures;
using Prism.Commands;
using Prism.Events;
using Prism.Services.Dialogs;
using RaceControl.Core.Mvvm;
using RaceControl.Events;
using RaceControl.Services.Interfaces.F1TV;
using RaceControl.Services.Interfaces.F1TV.Api;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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

        private ICommand _mouseEnterCommand;
        private ICommand _mouseLeaveCommand;
        private ICommand _mouseDownCommand;
        private ICommand _togglePauseCommand;
        private ICommand _syncSessionCommand;
        private ICommand _fastForwardCommand;
        private ICommand _toggleFullScreenCommand;
        private ICommand _audioTrackSelectionChangedCommand;
        private ICommand _castVideoCommand;

        private string _token;
        private Session _session;
        private Channel _channel;
        private SubscriptionToken _syncSessionSubscriptionToken;
        private MediaPlayer _mediaPlayer;
        private MediaPlayer _mediaPlayerCast;
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

        public override string Title => $"{_session} - {_channel}";

        public ICommand MouseEnterCommand => _mouseEnterCommand ??= new DelegateCommand<MouseEventArgs>(MouseEnterExecute);
        public ICommand MouseLeaveCommand => _mouseLeaveCommand ??= new DelegateCommand<MouseEventArgs>(MouseLeaveExecute);
        public ICommand MouseDownCommand => _mouseDownCommand ??= new DelegateCommand<MouseButtonEventArgs>(MouseDownExecute);
        public ICommand TogglePauseCommand => _togglePauseCommand ??= new DelegateCommand(TogglePauseExecute);
        public ICommand SyncSessionCommand => _syncSessionCommand ??= new DelegateCommand(SyncSessionExecute);
        public ICommand FastForwardCommand => _fastForwardCommand ??= new DelegateCommand(FastForwardExecute);
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

            MediaPlayer = CreateMediaPlayer();
            MediaPlayer.ESAdded += MediaPlayer_ESAdded;
            MediaPlayer.ESDeleted += MediaPlayer_ESDeleted;

            if (MediaPlayer.Play(await CreatePlaybackMedia()))
            {
                _syncSessionSubscriptionToken = _eventAggregator.GetEvent<SyncSessionEvent>().Subscribe(OnSyncSession);
            }

            _rendererDiscoverer = new RendererDiscoverer(_libVLC);
            _rendererDiscoverer.ItemAdded += RendererDiscoverer_ItemAdded;
            _rendererDiscoverer.Start();
        }

        public override void OnDialogClosed()
        {
            base.OnDialogClosed();

            _rendererDiscoverer.ItemAdded -= RendererDiscoverer_ItemAdded;
            _rendererDiscoverer.Stop();

            if (_syncSessionSubscriptionToken != null)
            {
                _eventAggregator.GetEvent<SyncSessionEvent>().Unsubscribe(_syncSessionSubscriptionToken);
            }

            MediaPlayer.ESAdded -= MediaPlayer_ESAdded;
            MediaPlayer.ESDeleted -= MediaPlayer_ESDeleted;
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

        private void MouseEnterExecute(MouseEventArgs args)
        {
            ShowControls = true;
        }

        private void MouseLeaveExecute(MouseEventArgs args)
        {
            ShowControls = false;
        }

        private void MouseDownExecute(MouseButtonEventArgs args)
        {
            if (args.ClickCount == 2)
            {
                ToggleFullScreenExecute();
            }
        }

        private void TogglePauseExecute()
        {
            if (MediaPlayer.CanPause)
            {
                MediaPlayer.Pause();
            }
        }

        private void SyncSessionExecute()
        {
            var payload = new SyncSessionEventPayload(_session.UID, MediaPlayer.Time);
            _eventAggregator.GetEvent<SyncSessionEvent>().Publish(payload);
        }

        private void FastForwardExecute()
        {
            ChangeTime(60);
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

            if (_mediaPlayerCast.Play(media) && MediaPlayer.IsPlaying)
            {
                _mediaPlayerCast.Time = MediaPlayer.Time;
            }
        }

        private void OnSyncSession(SyncSessionEventPayload payload)
        {
            if (_session.UID == payload.SessionUID)
            {
                if (MediaPlayer.IsPlaying)
                {
                    MediaPlayer.Time = payload.Time;
                }

                if (_mediaPlayerCast != null && _mediaPlayerCast.IsPlaying)
                {
                    _mediaPlayerCast.Time = payload.Time;
                }
            }
        }

        private void ChangeTime(int seconds)
        {
            if (MediaPlayer.IsPlaying)
            {
                MediaPlayer.Time = MediaPlayer.Time + (seconds * 1000);
            }
        }

        private void SetFullScreen()
        {
            FullScreen = true;
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            WindowState = WindowState.Maximized;
        }

        private void SetWindowed()
        {
            FullScreen = false;
            WindowStyle = WindowStyle.SingleBorderWindow;
            ResizeMode = ResizeMode.CanResize;
            WindowState = WindowState.Normal;
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
            var url = await _apiService.GetTokenisedUrlForChannelAsync(_token, _channel.Self);
            var media = new Media(_libVLC, url, FromType.FromLocation);

            return media;
        }
    }
}