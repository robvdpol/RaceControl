using NLog;
using Prism.Services.Dialogs;
using RaceControl.Common.Enum;
using RaceControl.Common.Interfaces;
using RaceControl.Core.Mvvm;
using RaceControl.Services.Interfaces.F1TV;
using RaceControl.Streamlink;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RaceControl.ViewModels
{
    public class DownloadDialogViewModel : DialogViewModelBase
    {
        private readonly IApiService _apiService;
        private readonly IStreamlinkLauncher _streamlinkLauncher;

        private Process _downloadProcess;
        private string _name;
        private string _filename;
        private bool _hasStarted;
        private bool _hasExited;
        private bool _hasFailed;

        public DownloadDialogViewModel(ILogger logger, IApiService apiService, IStreamlinkLauncher streamlinkLauncher) : base(logger)
        {
            _apiService = apiService;
            _streamlinkLauncher = streamlinkLauncher;
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

        public override async void OnDialogOpened(IDialogParameters parameters)
        {
            Title = "Download";
            Name = parameters.GetValue<string>(ParameterNames.NAME);
            Filename = parameters.GetValue<string>(ParameterNames.FILENAME);
            var token = parameters.GetValue<string>(ParameterNames.TOKEN);
            var playable = parameters.GetValue<IPlayable>(ParameterNames.PLAYABLE);
            var streamUrl = await GenerateStreamUrlAsync(token, playable.ContentType, playable.ContentURL);

            if (streamUrl == null)
            {
                HasFailed = true;
            }
            else
            {
                Logger.Info($"Starting download process for content-type '{playable.ContentType}' and content-URL '{playable.ContentURL}'...");
                _downloadProcess = _streamlinkLauncher.StartStreamlinkDownload(streamUrl, Filename, exitCode =>
                {
                    HasExited = true;
                    HasFailed = exitCode != 0;
                    Logger.Info($"Download process finished with exitcode '{exitCode}'.");
                });
                HasStarted = true;
            }

            base.OnDialogOpened(parameters);
        }

        public override void OnDialogClosed()
        {
            CleanupProcess(_downloadProcess);

            base.OnDialogClosed();
        }

        private async Task<string> GenerateStreamUrlAsync(string token, ContentType contentType, string contentUrl)
        {
            try
            {
                return await _apiService.GetTokenisedUrlAsync(token, contentType, contentUrl);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "An error occurred while trying to get tokenised URL.");
            }

            return null;
        }
    }
}