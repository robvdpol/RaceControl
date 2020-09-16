using DryIoc;
using LibVLCSharp.Shared;
using Newtonsoft.Json;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using Prism.DryIoc;
using Prism.Ioc;
using RaceControl.Common.Interfaces;
using RaceControl.Common.Utils;
using RaceControl.Core.Helpers;
using RaceControl.Core.Mvvm;
using RaceControl.Core.Settings;
using RaceControl.Core.Streamlink;
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
using RaceControl.ViewModels;
using RaceControl.Views;
using RaceControl.Vlc;
using System;
using System.Windows;
using System.Windows.Threading;
using LibVLCSharpCore = LibVLCSharp.Shared.Core;
using LogLevelNLog = NLog.LogLevel;
using LogLevelVLC = LibVLCSharp.Shared.LogLevel;

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
            var splashScreen = new SplashScreen("splashscreen.png");
            splashScreen.Show(false);
            LibVLCSharpCore.Initialize();
            InitializeLogging();
            base.Initialize();
            splashScreen.Close(TimeSpan.Zero);
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.GetContainer().Register(Made.Of<ILogger>(() => LogManager.GetLogger(Arg.Index<string>(0)), request => request.Parent.ImplementationType.Name));
            containerRegistry.RegisterDialogWindow<DialogWindow>();
            containerRegistry.RegisterDialogWindow<VideoDialogWindow>(nameof(VideoDialogWindow));
            containerRegistry.RegisterDialog<LoginDialog, LoginDialogViewModel>();
            containerRegistry.RegisterDialog<UpgradeDialog, UpgradeDialogViewModel>();
            containerRegistry.RegisterDialog<DownloadDialog, DownloadDialogViewModel>();
            containerRegistry.RegisterDialog<VideoDialog, VideoDialogViewModel>();

            containerRegistry
                .RegisterSingleton<IExtendedDialogService, ExtendedDialogService>()
                .RegisterSingleton<IRestClient, RestClient>()
                .RegisterSingleton<ISettings, Settings>()
                .RegisterSingleton<IVideoDialogLayout, VideoDialogLayout>()
                .Register<JsonSerializer>(() => new JsonSerializer { Formatting = Formatting.Indented })
                .Register<IAuthorizationService, AuthorizationService>()
                .Register<IF1TVClient, F1TVClient>()
                .Register<IApiService, ApiService>()
                .Register<IGithubService, GithubService>()
                .Register<ICredentialService, CredentialService>()
                .Register<IStreamlinkLauncher, StreamlinkLauncher>()
                .Register<IMediaPlayer, VlcMediaPlayer>()
                .Register<IMediaDownloader, VlcMediaDownloader>()
                .Register<MediaPlayer>(CreateMediaPlayer)
                .RegisterInstance(CreateLibVLC());
        }

        private static MediaPlayer CreateMediaPlayer(IContainerProvider container)
        {
            return new MediaPlayer(container.Resolve<LibVLC>())
            {
                EnableHardwareDecoding = true,
                EnableMouseInput = false,
                EnableKeyInput = false,
                FileCaching = 5000,
                NetworkCaching = 10000
            };
        }

        private static LibVLC CreateLibVLC()
        {
            var libVLC = new LibVLC();
            var logger = LogManager.GetLogger(libVLC.GetType().FullName);

            libVLC.Log += (sender, args) =>
            {
                switch (args.Level)
                {
                    case LogLevelVLC.Debug:
                        logger.Debug($"[VLC] {args.Message}");
                        break;

                    case LogLevelVLC.Notice:
                        logger.Info($"[VLC] {args.Message}");
                        break;

                    case LogLevelVLC.Warning:
                        logger.Warn($"[VLC] {args.Message}");
                        break;

                    case LogLevelVLC.Error:
                        logger.Error($"[VLC] {args.Message}");
                        break;
                }
            };

            return libVLC;
        }

        private static void InitializeLogging()
        {
            var config = new LoggingConfiguration();
            var logfile = new FileTarget("logfile")
            {
                FileName = FolderUtils.GetLocalApplicationDataFilename("RaceControl.log"),
                Layout = Layout.FromString("${longdate} ${uppercase:${level}} ${message}${onexception:inner=${newline}${exception:format=tostring}}"),
                ArchiveAboveSize = 1024 * 1024,
                ArchiveNumbering = ArchiveNumberingMode.Rolling,
                MaxArchiveFiles = 2
            };
            config.AddRule(LogLevelNLog.Info, LogLevelNLog.Fatal, logfile);
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