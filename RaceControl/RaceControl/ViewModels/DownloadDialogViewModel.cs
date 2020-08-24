using NLog;
using Prism.Services.Dialogs;
using RaceControl.Core.Helpers;
using RaceControl.Core.Mvvm;
using RaceControl.Services.Interfaces.F1TV;
using RaceControl.Streamlink;
using System;
using System.Diagnostics;

namespace RaceControl.ViewModels
{
    public class DownloadDialogViewModel : DialogViewModelBase
    {
        private readonly ILogger _logger;
        private readonly IApiService _apiService;
        private readonly IStreamlinkLauncher _streamlinkLauncher;

        private Process _downloadProcess;
        private string _name;
        private string _filename;
        private bool _hasExited;
        private bool _exitCodeSuccess;

        public DownloadDialogViewModel(ILogger logger, IApiService apiService, IStreamlinkLauncher streamlinkLauncher)
        {
            _logger = logger;
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

        public bool HasExited
        {
            get => _hasExited;
            set => SetProperty(ref _hasExited, value);
        }

        public bool ExitCodeSuccess
        {
            get => _exitCodeSuccess;
            set => SetProperty(ref _exitCodeSuccess, value);
        }

        public override async void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);

            Title = "Download";
            Name = parameters.GetValue<string>(ParameterNames.NAME);
            Filename = parameters.GetValue<string>(ParameterNames.FILENAME);
            var token = parameters.GetValue<string>(ParameterNames.TOKEN);
            var contentType = parameters.GetValue<ContentType>(ParameterNames.CONTENT_TYPE);
            var contentUrl = parameters.GetValue<string>(ParameterNames.CONTENT_URL);

            string streamUrl;

            try
            {
                streamUrl = await _apiService.GetTokenisedUrlAsync(token, contentType, contentUrl);
            }
            catch (Exception ex)
            {
                var message = $"An error occurred while trying to get tokenised URL for content-type '{contentType}' and content-URL '{contentUrl}'.";
                _logger.Error(ex, message);
                MessageBoxHelper.ShowError(message);
                return;
            }

            _downloadProcess = _streamlinkLauncher.StartStreamlinkDownload(streamUrl, Filename, DownloadProcess_Exited);
        }

        public override void OnDialogClosed()
        {
            CleanupProcess(_downloadProcess);

            base.OnDialogClosed();
        }

        private void DownloadProcess_Exited(int exitCode)
        {
            HasExited = true;
            ExitCodeSuccess = exitCode == 0;
        }
    }
}