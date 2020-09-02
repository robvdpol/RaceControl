using LibVLCSharp.Shared;
using NLog;
using Prism.Services.Dialogs;
using RaceControl.Common.Interfaces;
using RaceControl.Core.Mvvm;
using RaceControl.Services.Interfaces.F1TV;
using System;
using System.Threading.Tasks;

namespace RaceControl.ViewModels
{
    public class DownloadDialogViewModel : DialogViewModelBase
    {
        private readonly IApiService _apiService;
        private readonly LibVLC _libVLC;
        private readonly MediaPlayer _mediaPlayer;

        private string _name;
        private string _filename;
        private bool _hasStarted;
        private bool _hasExited;
        private bool _hasFailed;
        private float _percentage;

        public DownloadDialogViewModel(ILogger logger, IApiService apiService, LibVLC libVLC, MediaPlayer mediaPlayer) : base(logger)
        {
            _apiService = apiService;
            _libVLC = libVLC;
            _mediaPlayer = mediaPlayer;
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Filename
        {
            get => _filename;
            set => SetProperty(ref _filename, value);
        }

        public bool HasStarted
        {
            get => _hasStarted;
            set => SetProperty(ref _hasStarted, value);
        }

        public bool HasExited
        {
            get => _hasExited;
            set => SetProperty(ref _hasExited, value);
        }

        public bool HasFailed
        {
            get => _hasFailed;
            set => SetProperty(ref _hasFailed, value);
        }

        public float Percentage
        {
            get => _percentage;
            set => SetProperty(ref _percentage, value);
        }

        public override async void OnDialogOpened(IDialogParameters parameters)
        {
            Title = "Download";
            Name = parameters.GetValue<string>(ParameterNames.NAME);
            Filename = parameters.GetValue<string>(ParameterNames.FILENAME);
            var token = parameters.GetValue<string>(ParameterNames.TOKEN);
            var playable = parameters.GetValue<IPlayable>(ParameterNames.PLAYABLE);
            var streamUrl = await GenerateStreamUrlAsync(token, playable);

            CreateMediaPlayer();

            if (streamUrl == null)
            {
                HasFailed = true;
            }
            else
            {
                Logger.Info($"Starting download process for content-type '{playable.ContentType}' and content-URL '{playable.ContentUrl}'...");

                var media = new Media(_libVLC, streamUrl, FromType.FromLocation);
                media.AddOption(":sout=#transcode{scodec=none}:std{access=file,mux=ts,dst='" + Filename + "'}");

                if (_mediaPlayer.Play(media))
                {
                    HasStarted = true;
                }
                else
                {
                    HasFailed = true;
                }
            }

            base.OnDialogOpened(parameters);
        }

        public override void OnDialogClosed()
        {
            RemoveMediaPlayer();

            base.OnDialogClosed();
        }

        private void CreateMediaPlayer()
        {
            _mediaPlayer.PositionChanged += MediaPlayer_PositionChanged;
            _mediaPlayer.EndReached += MediaPlayer_EndReached;
        }

        private void RemoveMediaPlayer()
        {
            _mediaPlayer.PositionChanged -= MediaPlayer_PositionChanged;
            _mediaPlayer.EndReached -= MediaPlayer_EndReached;
            _mediaPlayer.Dispose();
        }

        private void MediaPlayer_PositionChanged(object sender, MediaPlayerPositionChangedEventArgs e)
        {
            Percentage = e.Position * 100;
        }

        private void MediaPlayer_EndReached(object sender, EventArgs e)
        {
            Percentage = 100;
            HasExited = true;
        }

        private async Task<string> GenerateStreamUrlAsync(string token, IPlayable playable)
        {
            try
            {
                return await _apiService.GetTokenisedUrlAsync(token, playable.ContentType, playable.ContentUrl);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "An error occurred while trying to get tokenised URL.");
            }

            return null;
        }
    }
}