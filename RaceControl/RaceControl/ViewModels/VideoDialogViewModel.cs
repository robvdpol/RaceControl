using LibVLCSharp.Shared;
using Prism.Services.Dialogs;
using RaceControl.Core.Mvvm;

namespace RaceControl.ViewModels
{
    public class VideoDialogViewModel : DialogViewModelBase
    {
        private readonly LibVLC _libVLC;

        private MediaPlayer _mediaPlayer;

        public VideoDialogViewModel(LibVLC libVLC)
        {
            _libVLC = libVLC;
        }

        public override string Title => "Video";

        public MediaPlayer MediaPlayer
        {
            get => _mediaPlayer ?? (_mediaPlayer = new MediaPlayer(_libVLC) { EnableHardwareDecoding = true });
            set => SetProperty(ref _mediaPlayer, value);
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);

            var url = parameters.GetValue<string>("url");
            MediaPlayer.Play(new Media(_libVLC, url, FromType.FromLocation));
        }

        public override void OnDialogClosed()
        {
            base.OnDialogClosed();

            MediaPlayer.Stop();
            MediaPlayer.Dispose();
        }
    }
}