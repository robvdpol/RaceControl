using LibVLCSharp.Shared;
using Prism.Ioc;
using RaceControl.Services;
using RaceControl.Services.F1TV;
using RaceControl.Services.Interfaces;
using RaceControl.Services.Interfaces.F1TV;
using RaceControl.Services.Interfaces.Lark;
using RaceControl.Services.Lark;
using RaceControl.ViewModels;
using RaceControl.Views;
using System.Windows;
using LibVLCSharpCore = LibVLCSharp.Shared.Core;

namespace RaceControl
{
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        public override void Initialize()
        {
            base.Initialize();
            LibVLCSharpCore.Initialize();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterDialog<LoginDialog, LoginDialogViewModel>();
            containerRegistry.RegisterDialog<VideoDialog, VideoDialogViewModel>();

            containerRegistry.RegisterSingleton<LibVLC>();
            containerRegistry.RegisterSingleton<IRestClient, RestClient>();
            containerRegistry.RegisterSingleton<IAuthorizationService, AuthorizationService>();
            containerRegistry.RegisterSingleton<IApiService, ApiService>();
            containerRegistry.Register<IF1TVClient, F1TVClient>();
        }
    }
}