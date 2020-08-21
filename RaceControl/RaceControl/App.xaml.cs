using DryIoc;
using LibVLCSharp.Shared;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using Prism.DryIoc;
using Prism.Ioc;
using RaceControl.Common;
using RaceControl.Common.Settings;
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
using LibVLCSharpCore = LibVLCSharp.Shared.Core;
using LogLevel = NLog.LogLevel;

namespace RaceControl
{
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void Initialize()
        {
            LibVLCSharpCore.Initialize();
            InitializeLogging();
            base.Initialize();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.GetContainer().Register(Made.Of<ILogger>(() => LogManager.GetLogger(Arg.Index<string>(0)), request => request.Parent.ImplementationType.Name));

            containerRegistry.RegisterDialogWindow<DialogWindow>();
            containerRegistry.RegisterDialog<LoginDialog, LoginDialogViewModel>();
            containerRegistry.RegisterDialog<UpgradeDialog, UpgradeDialogViewModel>();
            containerRegistry.RegisterDialog<VideoDialog, VideoDialogViewModel>();

            containerRegistry.RegisterInstance(new LibVLC());
            containerRegistry.RegisterSingleton<IExtendedDialogService, ExtendedDialogService>();
            containerRegistry.RegisterSingleton<IChildProcessTracker, ChildProcessTracker>();
            containerRegistry.RegisterSingleton<IRestClient, RestClient>();
            containerRegistry.RegisterSingleton<ISettings, Settings>();
            containerRegistry.Register<IAuthorizationService, AuthorizationService>();
            containerRegistry.Register<IF1TVClient, F1TVClient>();
            containerRegistry.Register<IApiService, ApiService>();
            containerRegistry.Register<IGithubService, GithubService>();
            containerRegistry.Register<ICredentialService, CredentialService>();
            containerRegistry.Register<IStreamlinkLauncher, StreamlinkLauncher>();
        }

        private static void InitializeLogging()
        {
            var config = new LoggingConfiguration();
            var logfile = new FileTarget("logfile")
            {
                FileName = "RaceControl.log",
                Layout = Layout.FromString("${longdate} ${uppercase:${level}} ${message}${onexception:inner=${newline}${exception:format=tostring}}"),
                ArchiveAboveSize = 5 * 1024 * 1024,
                ArchiveNumbering = ArchiveNumberingMode.Rolling,
                MaxArchiveFiles = 2
            };
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logfile);
            LogManager.Configuration = config;
        }

        private void PrismApplication_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LogManager.GetCurrentClassLogger().Error(e.Exception, "An unhandled exception occurred.");
            MessageBoxHelper.ShowError(e.Exception.Message);
            e.Handled = true;
        }
    }
}