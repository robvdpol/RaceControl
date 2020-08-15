using LibVLCSharp.Shared;
using Prism.Ioc;
using RaceControl.Common;
using RaceControl.Core.Helpers;
using RaceControl.Core.Mvvm;
using RaceControl.Services;
using RaceControl.Services.Credential;
using RaceControl.Services.F1TV;
using RaceControl.Services.Github;
using RaceControl.Services.Interfaces;
using RaceControl.Services.Interfaces.Credential;
using RaceControl.Services.Interfaces.F1TV;
using RaceControl.Services.Interfaces.Github;
using RaceControl.Services.Interfaces.Lark;
using RaceControl.Services.Lark;
using RaceControl.Streamlink;
using RaceControl.ViewModels;
using RaceControl.Views;
using System.Windows;
using System.Windows.Threading;

namespace RaceControl
{
    public partial class App
    {
        protected override Window CreateShell()
        {
            LibVLCSharp.Shared.Core.Initialize();

            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterDialogWindow<DialogWindow>();
            containerRegistry.RegisterDialog<LoginDialog, LoginDialogViewModel>();
            containerRegistry.RegisterDialog<UpgradeDialog, UpgradeDialogViewModel>();
            containerRegistry.RegisterDialog<VideoDialog, VideoDialogViewModel>();

            containerRegistry.RegisterSingleton<IExtendedDialogService, ExtendedDialogService>();
            containerRegistry.RegisterSingleton<LibVLC>();
            containerRegistry.RegisterSingleton<IChildProcessTracker, ChildProcessTracker>();
            containerRegistry.RegisterSingleton<IRestClient, RestClient>();
            containerRegistry.Register<IAuthorizationService, AuthorizationService>();
            containerRegistry.Register<IF1TVClient, F1TVClient>();
            containerRegistry.Register<IApiService, ApiService>();
            containerRegistry.Register<IGithubService, GithubService>();
            containerRegistry.Register<ICredentialService, CredentialService>();
            containerRegistry.Register<IStreamlinkLauncher, StreamlinkLauncher>();
        }

        private void PrismApplication_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBoxHelper.ShowError(e.Exception.Message);
            e.Handled = true;
        }
    }
}