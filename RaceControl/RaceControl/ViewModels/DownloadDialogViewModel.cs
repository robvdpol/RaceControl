using NLog;
using Prism.Services.Dialogs;
using RaceControl.Common.Enums;
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

        public override string Title => "Download";

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
            var subscriptionToken = parameters.GetValue<string>(ParameterNames.SubscriptionToken);
            PlayableContent = parameters.GetValue<IPlayableContent>(ParameterNames.Content);
            Filename = parameters.GetValue<string>(ParameterNames.Filename);
            StartDownloadAsync(subscriptionToken).Await(DownloadStarted, DownloadFailed);
        }

        public override void OnDialogClosed()
        {
            MediaDownloader.Dispose();
            base.OnDialogClosed();
        }

        private async Task StartDownloadAsync(string subscriptionToken)
        {
            var streamUrl = await _apiService.GetTokenisedUrlAsync(subscriptionToken, PlayableContent);
            await MediaDownloader.StartDownloadAsync(streamUrl, Filename);
        }

        private void DownloadStarted()
        {
            base.OnDialogOpened(null);
            MediaDownloader.SetDownloadStatus(DownloadStatus.Downloading);
        }

        private void DownloadFailed(Exception ex)
        {
            base.OnDialogOpened(null);
            MediaDownloader.SetDownloadStatus(DownloadStatus.Failed);
            HandleCriticalError(ex);
        }
    }
}