using NLog;
using Prism.Services.Dialogs;
using RaceControl.Common.Enums;
using RaceControl.Common.Interfaces;
using RaceControl.Core.Mvvm;
using RaceControl.Core.Settings;
using RaceControl.Services.Interfaces.F1TV;
using System.Threading.Tasks;

namespace RaceControl.ViewModels
{
    public class DownloadDialogViewModel : DialogViewModelBase
    {
        private readonly ISettings _settings;
        private readonly IApiService _apiService;

        private IPlayableContent _playableContent;
        private string _filename;

        public DownloadDialogViewModel(ILogger logger, ISettings settings, IApiService apiService, IMediaDownloader mediaDownloader) : base(logger)
        {
            _settings = settings;
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
            var token = parameters.GetValue<string>(ParameterNames.Token);
            PlayableContent = parameters.GetValue<IPlayableContent>(ParameterNames.Content);
            Filename = parameters.GetValue<string>(ParameterNames.Filename);
            GetTokenisedUrlAndStartDownloadAsync(token).Await(ex =>
            {
                HandleNonCriticalError(ex);
                MediaDownloader.SetDownloadStatus(DownloadStatus.Failed);
            });

            base.OnDialogOpened(parameters);
        }

        public override void OnDialogClosed()
        {
            MediaDownloader.StopDownload();
            MediaDownloader.Dispose();

            base.OnDialogClosed();
        }

        private async Task GetTokenisedUrlAndStartDownloadAsync(string token)
        {
            var streamUrl = await _apiService.GetTokenisedUrlAsync(token, _settings.StreamType, PlayableContent);
            await MediaDownloader.StartDownloadAsync(streamUrl, Filename);
        }
    }
}