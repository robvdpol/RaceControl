using NLog;
using Prism.Services.Dialogs;
using RaceControl.Common.Interfaces;
using RaceControl.Core.Mvvm;
using RaceControl.Interfaces;
using RaceControl.Services.Interfaces.F1TV;

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

        public override async void OnDialogOpened(IDialogParameters parameters)
        {
            var token = parameters.GetValue<string>(ParameterNames.TOKEN);
            PlayableContent = parameters.GetValue<IPlayableContent>(ParameterNames.PLAYABLE_CONTENT);
            Filename = parameters.GetValue<string>(ParameterNames.FILENAME);

            var (success, streamUrl) = await _apiService.TryGetTokenisedUrlAsync(token, PlayableContent);

            if (success)
            {
                await MediaDownloader.StartDownloadAsync(streamUrl, Filename);
            }
            else
            {
                MediaDownloader.SetFailedStatus();
            }

            base.OnDialogOpened(parameters);
        }

        public override void OnDialogClosed()
        {
            MediaDownloader.Dispose();

            base.OnDialogClosed();
        }
    }
}