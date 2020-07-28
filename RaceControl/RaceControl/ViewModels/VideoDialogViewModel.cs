using LibVLCSharp.Shared;
using LibVLCSharp.Shared.Structures;
using Prism.Commands;
using Prism.Services.Dialogs;
using RaceControl.Core.Mvvm;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace RaceControl.ViewModels
{
    public class VideoDialogViewModel : DialogViewModelBase
    {
        private readonly LibVLC _libVLC;

        private ICommand _togglePauseCommand;
        private MediaPlayer _mediaPlayer;
        private ObservableCollection<TrackDescription> _trackDescriptions;

        public VideoDialogViewModel(LibVLC libVLC)
        {
            _libVLC = libVLC;
        }

        public override string Title => "Video";

        public ICommand TogglePauseCommand => _togglePauseCommand ?? (_togglePauseCommand = new DelegateCommand(TogglePauseExecute));

        public MediaPlayer MediaPlayer
        {
            get => _mediaPlayer ?? (_mediaPlayer = new MediaPlayer(_libVLC) { EnableHardwareDecoding = true });
            set => SetProperty(ref _mediaPlayer, value);
        }

        public ObservableCollection<TrackDescription> TrackDescriptions
        {
            get => _trackDescriptions;
            set => SetProperty(ref _trackDescriptions, value);
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);

            var url = parameters.GetValue<string>("url");
            var media = new Media(_libVLC, url, FromType.FromLocation);
            MediaPlayer.Play(media);
            //TrackDescriptions = new ObservableCollection<TrackDescription>(MediaPlayer.AudioTrackDescription);
        }

        public override void OnDialogClosed()
        {
            base.OnDialogClosed();

            MediaPlayer.Stop();
            MediaPlayer.Dispose();
        }

        private void TogglePauseExecute()
        {
            if (MediaPlayer.CanPause)
            {
                MediaPlayer.Pause();
            }
        }
    }
}