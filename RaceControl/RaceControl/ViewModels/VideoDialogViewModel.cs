using LibVLCSharp.Shared;
using LibVLCSharp.Shared.Structures;
using Prism.Commands;
using Prism.Events;
using Prism.Services.Dialogs;
using RaceControl.Core.Mvvm;
using RaceControl.Events;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RaceControl.ViewModels
{
    public class VideoDialogViewModel : DialogViewModelBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly LibVLC _libVLC;

        private ICommand _togglePauseCommand;
        private ICommand _syncVideoCommand;
        private ICommand _audioTrackSelectionChangedCommand;
        private ICommand _videoTrackSelectionChangedCommand;
        private ICommand _castVideoCommand;

        private Media _media;
        private MediaPlayer _mediaPlayer;
        private RendererDiscoverer _rendererDiscoverer;
        private ObservableCollection<TrackDescription> _audioTrackDescriptions;
        private ObservableCollection<TrackDescription> _videoTrackDescriptions;
        private ObservableCollection<RendererItem> _rendererItems;
        private RendererItem _selectedRendererItem;

        public VideoDialogViewModel(IEventAggregator eventAggregator, LibVLC libVLC)
        {
            _eventAggregator = eventAggregator;
            _libVLC = libVLC;
        }

        public override string Title => "Video";

        public ICommand TogglePauseCommand => _togglePauseCommand ?? (_togglePauseCommand = new DelegateCommand(TogglePauseExecute));
        public ICommand SyncVideoCommand => _syncVideoCommand ?? (_syncVideoCommand = new DelegateCommand(SyncVideoExecute));
        public ICommand AudioTrackSelectionChangedCommand => _audioTrackSelectionChangedCommand ?? (_audioTrackSelectionChangedCommand = new DelegateCommand<SelectionChangedEventArgs>(AudioTrackSelectionChangedExecute));
        public ICommand VideoTrackSelectionChangedCommand => _videoTrackSelectionChangedCommand ?? (_videoTrackSelectionChangedCommand = new DelegateCommand<SelectionChangedEventArgs>(VideoTrackSelectionChangedExecute));
        public ICommand CastVideoCommand => _castVideoCommand ?? (_castVideoCommand = new DelegateCommand(CastVideoExecute, CastVideoCanExecute).ObservesProperty(() => SelectedRendererItem));

        public Media Media
        {
            get => _media;
            set => SetProperty(ref _media, value);
        }

        public MediaPlayer MediaPlayer
        {
            get => _mediaPlayer;
            set => SetProperty(ref _mediaPlayer, value);
        }

        public RendererDiscoverer RendererDiscoverer
        {
            get => _rendererDiscoverer;
            set => SetProperty(ref _rendererDiscoverer, value);
        }

        public ObservableCollection<TrackDescription> AudioTrackDescriptions
        {
            get => _audioTrackDescriptions ?? (_audioTrackDescriptions = new ObservableCollection<TrackDescription>());
            set => SetProperty(ref _audioTrackDescriptions, value);
        }

        public ObservableCollection<TrackDescription> VideoTrackDescriptions
        {
            get => _videoTrackDescriptions ?? (_videoTrackDescriptions = new ObservableCollection<TrackDescription>());
            set => SetProperty(ref _videoTrackDescriptions, value);
        }

        public ObservableCollection<RendererItem> RendererItems
        {
            get => _rendererItems ?? (_rendererItems = new ObservableCollection<RendererItem>());
            set => SetProperty(ref _rendererItems, value);
        }

        public RendererItem SelectedRendererItem
        {
            get => _selectedRendererItem;
            set => SetProperty(ref _selectedRendererItem, value);
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);

            _eventAggregator.GetEvent<SyncVideoEvent>().Subscribe(SyncVideo);

            RendererDiscoverer = new RendererDiscoverer(_libVLC);
            RendererDiscoverer.ItemAdded += RendererDiscoverer_ItemAdded;
            RendererDiscoverer.Start();

            var url = parameters.GetValue<string>("url");
            Media = new Media(_libVLC, url, FromType.FromLocation);
            MediaPlayer = new MediaPlayer(_libVLC) { EnableHardwareDecoding = true };
            MediaPlayer.ESAdded += MediaPlayer_ESAdded;
            MediaPlayer.ESDeleted += MediaPlayer_ESDeleted;
            MediaPlayer.Play(Media);
        }

        public override void OnDialogClosed()
        {
            base.OnDialogClosed();

            RendererDiscoverer.ItemAdded -= RendererDiscoverer_ItemAdded;
            RendererDiscoverer.Stop();

            MediaPlayer.ESAdded -= MediaPlayer_ESAdded;
            MediaPlayer.ESDeleted -= MediaPlayer_ESDeleted;
            MediaPlayer.Stop();
            MediaPlayer.Dispose();
        }

        private void SyncVideo(SyncVideoEventPayload payload)
        {
            MediaPlayer.Time = payload.Time;
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

        private void CastVideoExecute()
        {
            var result = _mediaPlayer.SetRenderer(SelectedRendererItem);
        }
    }
}