using LibVLCSharp.Shared;
using LibVLCSharp.Shared.Structures;
using Prism.Commands;
using Prism.Services.Dialogs;
using RaceControl.Core.Mvvm;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RaceControl.ViewModels
{
    public class VideoDialogViewModel : DialogViewModelBase
    {
        private readonly LibVLC _libVLC;

        private ICommand _togglePauseCommand;
        private ICommand _audioTrackSelectionChangedCommand;

        private Media _media;
        private MediaPlayer _mediaPlayer;
        private ObservableCollection<TrackDescription> _audioTrackDescriptions;

        public VideoDialogViewModel(LibVLC libVLC)
        {
            _libVLC = libVLC;
        }

        public override string Title => "Video";

        public ICommand TogglePauseCommand => _togglePauseCommand ?? (_togglePauseCommand = new DelegateCommand(TogglePauseExecute));
        public ICommand AudioTrackSelectionChangedCommand => _audioTrackSelectionChangedCommand ?? (_audioTrackSelectionChangedCommand = new DelegateCommand<SelectionChangedEventArgs>(AudioTrackSelectionChangedExecute));

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

        public ObservableCollection<TrackDescription> AudioTrackDescriptions
        {
            get => _audioTrackDescriptions ?? (_audioTrackDescriptions = new ObservableCollection<TrackDescription>());
            set => SetProperty(ref _audioTrackDescriptions, value);
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);

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

            MediaPlayer.ESAdded -= MediaPlayer_ESAdded;
            MediaPlayer.ESDeleted -= MediaPlayer_ESDeleted;
            MediaPlayer.Stop();
            MediaPlayer.Dispose();
        }

        private void MediaPlayer_ESAdded(object sender, MediaPlayerESAddedEventArgs e)
        {
            if (e.Type == TrackType.Audio && e.Id >= 0)
            {
                var description = MediaPlayer.AudioTrackDescription.First(p => p.Id == e.Id);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    AudioTrackDescriptions.Add(description);
                });
            }
        }

        private void MediaPlayer_ESDeleted(object sender, MediaPlayerESDeletedEventArgs e)
        {
            if (e.Type == TrackType.Audio && e.Id >= 0)
            {
                var description = AudioTrackDescriptions.First(p => p.Id == e.Id);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    AudioTrackDescriptions.Remove(description);
                });
            }
        }

        private void TogglePauseExecute()
        {
            if (MediaPlayer.CanPause)
            {
                MediaPlayer.Pause();
            }
        }

        private void AudioTrackSelectionChangedExecute(SelectionChangedEventArgs e)
        {
            var trackDescription = (TrackDescription)e.AddedItems[0];
            MediaPlayer.SetAudioTrack(trackDescription.Id);
        }
    }
}