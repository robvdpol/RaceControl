using Prism.Services.Dialogs;
using RaceControl.Core.Mvvm;
using RaceControl.Streamlink;
using System.Diagnostics;

namespace RaceControl.ViewModels
{
    public class DownloadDialogViewModel : DialogViewModelBase
    {
        private readonly IStreamlinkLauncher _streamlinkLauncher;

        private Process _downloadProcess;
        private string _name;
        private bool _hasExited;
        private bool _exitCodeSuccess;

        public DownloadDialogViewModel(IStreamlinkLauncher streamlinkLauncher)
        {
            _streamlinkLauncher = streamlinkLauncher;
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
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

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);

            Title = "Download";
            Name = parameters.GetValue<string>(ParameterNames.NAME);
            var streamUrl = parameters.GetValue<string>(ParameterNames.STREAM_URL);
            var filename = parameters.GetValue<string>(ParameterNames.FILENAME);
            _downloadProcess = _streamlinkLauncher.StartStreamlinkDownload(streamUrl, filename, DownloadProcess_Exited);
        }

        public override void OnDialogClosed()
        {
            base.OnDialogClosed();

            CleanupProcess(_downloadProcess);
        }

        private void DownloadProcess_Exited(int exitCode)
        {
            HasExited = true;
            ExitCodeSuccess = exitCode == 0;
        }
    }
}