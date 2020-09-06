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

        private string _token;
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

        public override async void OnDialogOpened(IDialogParameters parameters)
        {
            _token = parameters.GetValue<string>(ParameterNames.TOKEN);
            PlayableContent = parameters.GetValue<IPlayableContent>(ParameterNames.PLAYABLE_CONTENT);
            Filename = parameters.GetValue<string>(ParameterNames.FILENAME);

            Logger.Info($"Downloading '{PlayableContent.Title}' to file '{Filename}'...");
            var streamUrl = await GenerateStreamUrlAsync();
            await MediaDownloader.StartDownloadAsync(streamUrl, Filename);

            base.OnDialogOpened(parameters);
        }

        public override void OnDialogClosed()
        {
            MediaDownloader.Dispose();

            base.OnDialogClosed();
        }

        private async Task<string> GenerateStreamUrlAsync()
        {
            try
            {
                return await _apiService.GetTokenisedUrlAsync(_token, PlayableContent);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "An error occurred while trying to get tokenised URL.");
            }

            return null;
        }
    }
}