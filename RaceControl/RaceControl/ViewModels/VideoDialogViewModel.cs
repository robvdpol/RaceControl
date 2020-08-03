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

        private ICommand _togglePauseCommand;
        private ICommand _syncVideoCommand;
        private ICommand _audioTrackSelectionChangedCommand;
        private ICommand _videoTrackSelectionChangedCommand;
        private ICommand _castVideoCommand;

        private string _token;
        private Channel _channel;
        private MediaPlayer _mediaPlayer;
        private MediaPlayer _mediaPlayerCast;
        private RendererDiscoverer _rendererDiscoverer;
        private ObservableCollection<TrackDescription> _audioTrackDescriptions;
        private ObservableCollection<TrackDescription> _videoTrackDescriptions;
        private ObservableCollection<RendererItem> _rendererItems;
        private RendererItem _selectedRendererItem;

        public VideoDialogViewModel(IEventAggregator eventAggregator, IApiService apiService, LibVLC libVLC)
        {
            _eventAggregator = eventAggregator;
            _apiService = apiService;
            _libVLC = libVLC;
        }

        public override string Title => "Video";

        public ICommand TogglePauseCommand => _togglePauseCommand ??= new DelegateCommand(TogglePauseExecute);
        public ICommand SyncVideoCommand => _syncVideoCommand ??= new DelegateCommand(SyncVideoExecute);
        public ICommand AudioTrackSelectionChangedCommand => _audioTrackSelectionChangedCommand ??= new DelegateCommand<SelectionChangedEventArgs>(AudioTrackSelectionChangedExecute);
        public ICommand VideoTrackSelectionChangedCommand => _videoTrackSelectionChangedCommand ??= new DelegateCommand<SelectionChangedEventArgs>(VideoTrackSelectionChangedExecute);
        public ICommand CastVideoCommand => _castVideoCommand ??= new DelegateCommand(CastVideoExecute, CastVideoCanExecute).ObservesProperty(() => SelectedRendererItem);

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

        public ObservableCollection<TrackDescription> VideoTrackDescriptions
        {
            get => _videoTrackDescriptions ??= new ObservableCollection<TrackDescription>();
            set => SetProperty(ref _videoTrackDescriptions, value);
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

        public override async void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);

            _token = parameters.GetValue<string>("token");
            _channel = parameters.GetValue<Channel>("channel");
            _eventAggregator.GetEvent<SyncVideoEvent>().Subscribe(SyncVideo);

            _rendererDiscoverer = new RendererDiscoverer(_libVLC);
            _rendererDiscoverer.ItemAdded += RendererDiscoverer_ItemAdded;
            _rendererDiscoverer.Start();

            MediaPlayer = CreateMediaPlayer();
            MediaPlayer.ESAdded += MediaPlayer_ESAdded;
            MediaPlayer.ESDeleted += MediaPlayer_ESDeleted;
            MediaPlayer.Play(await CreatePlaybackMedia());
        }

        public override void OnDialogClosed()
        {
            base.OnDialogClosed();

            _rendererDiscoverer.ItemAdded -= RendererDiscoverer_ItemAdded;
            _rendererDiscoverer.Stop();

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

        private void SyncVideo(SyncVideoEventPayload payload)
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

                    case TrackType.Video:
                        var videoTrackDescription = MediaPlayer.VideoTrackDescription.First(p => p.Id == e.Id);

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            VideoTrackDescriptions.Add(videoTrackDescription);
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

                    case TrackType.Video:
                        var videoTrackDescription = VideoTrackDescriptions.First(p => p.Id == e.Id);

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            VideoTrackDescriptions.Remove(videoTrackDescription);
                        });
                        break;
                }
            }
        }

        private void TogglePauseExecute()
        {
            if (MediaPlayer.CanPause)
            {
                MediaPlayer.Pause();
            }
        }

        private void SyncVideoExecute()
        {
            // todo: only sync videos from same session
            var payload = new SyncVideoEventPayload(MediaPlayer.Time);
            _eventAggregator.GetEvent<SyncVideoEvent>().Publish(payload);
        }

        private void AudioTrackSelectionChangedExecute(SelectionChangedEventArgs args)
        {
            var trackDescription = (TrackDescription)args.AddedItems[0];
            MediaPlayer.SetAudioTrack(trackDescription.Id);
        }

        private void VideoTrackSelectionChangedExecute(SelectionChangedEventArgs args)
        {
            var trackDescription = (TrackDescription)args.AddedItems[0];
            MediaPlayer.SetVideoTrack(trackDescription.Id);
        }

        private bool CastVideoCanExecute()
        {
            return SelectedRendererItem != null;
        }

        private async void CastVideoExecute()
        {
            _mediaPlayerCast ??= CreateMediaPlayer();

            if (_mediaPlayerCast.IsPlaying)
            {
                _mediaPlayerCast.Stop();
            }

            _mediaPlayerCast.SetRenderer(SelectedRendererItem);
            _mediaPlayerCast.Play(await CreatePlaybackMedia());

            if (_mediaPlayer.IsPlaying)
            {
                _mediaPlayerCast.Time = _mediaPlayer.Time;
            }
        }

        private MediaPlayer CreateMediaPlayer()
        {
            return new MediaPlayer(_libVLC) { EnableHardwareDecoding = true };
        }

        private async Task<Media> CreatePlaybackMedia()
        {
            var url = await _apiService.GetTokenisedUrlForChannelAsync(_token, _channel.Self);

            return new Media(_libVLC, url, FromType.FromLocation);
        }
    }
}