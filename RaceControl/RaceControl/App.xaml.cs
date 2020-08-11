using LibVLCSharp.Shared;
using Prism.Ioc;
using RaceControl.Core.Helpers;
using RaceControl.Services;
using RaceControl.Services.F1TV;
using RaceControl.Services.Interfaces;
using RaceControl.Services.Interfaces.F1TV;
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
            containerRegistry.RegisterDialog<LoginDialog, LoginDialogViewModel>();
            containerRegistry.RegisterDialog<VideoDialog, VideoDialogViewModel>();

            containerRegistry.RegisterSingleton<LibVLC>();
            containerRegistry.RegisterSingleton<IStreamlinkLauncher, StreamlinkLauncher>();
            containerRegistry.RegisterSingleton<IRestClient, RestClient>();
            containerRegistry.RegisterSingleton<IAuthorizationService, AuthorizationService>();
            containerRegistry.RegisterSingleton<IApiService, ApiService>();
            containerRegistry.Register<IF1TVClient, F1TVClient>();
        }

        private void PrismApplication_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBoxHelper.ShowError(e.Exception.Message);
            e.Handled = true;
        }
    }
}