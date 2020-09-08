using NLog;
using Prism.Services.Dialogs;
using RaceControl.Common.Interfaces;
using RaceControl.Core.Mvvm;
using RaceControl.Interfaces;
using RaceControl.Services.Interfaces.F1TV;
using System;
using System.Threading.Tasks;

namespace RaceControl.ViewModels
{
    public class DownloadDialogViewModel : DialogViewModelBase
    {
        private readonly IApiService _apiService;

        private IPlayableContent _playableContent;
        private string _filename;

        public DownloadDialogViewModel(ILogger logger, IApiService apiService, IMediaDownloader mediaDownloader) : base(logger)
        {
            _apiService = apiService;
            MediaDownloader = mediaDownloader;
        }

        public override string Title { get; } = "Download";

        public IMediaDownloader MediaDownloader { get; }

        public IPlayableContent PlayableContent
        {
            get => _playableContent;
            set => SetProperty(ref _playableContent, value);
        }

        public string Filename
        {
            get => _filename;
            set => SetProperty(ref _filename, value);
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            var token = parameters.GetValue<string>(ParameterNames.TOKEN);
            PlayableContent = parameters.GetValue<IPlayableContent>(ParameterNames.PLAYABLE_CONTENT);
            Filename = parameters.GetValue<string>(ParameterNames.FILENAME);
            GetTokenisedUrlAndStartDownloadAsync(token).Await(HandleDownloadError);

            base.OnDialogOpened(parameters);
        }

        public override void OnDialogClosed()
        {
            MediaDownloader.Dispose();

            base.OnDialogClosed();
        }

        private async Task GetTokenisedUrlAndStartDownloadAsync(string token)
        {
            var streamUrl = await _apiService.GetTokenisedUrlAsync(token, PlayableContent);
            await MediaDownloader.StartDownloadAsync(streamUrl, Filename);
        }

        private void HandleDownloadError(Exception ex)
        {
            Logger.Error(ex, "An error occurred while trying to download content.");
            MediaDownloader.SetFailedStatus();
        }
    }
}