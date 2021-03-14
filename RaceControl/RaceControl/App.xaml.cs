using DryIoc;
using GoogleCast;
using LibVLCSharp.Shared;
using Newtonsoft.Json;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using Prism.DryIoc;
using Prism.Ioc;
using RaceControl.Common.Generators;
using RaceControl.Common.Interfaces;
using RaceControl.Common.Utils;
using RaceControl.Core.Helpers;
using RaceControl.Core.Settings;
using RaceControl.Extensions;
using RaceControl.Services.Credential;
using RaceControl.Services.F1TV;
using RaceControl.Services.Github;
using RaceControl.Services.Interfaces.Credential;
using RaceControl.Services.Interfaces.F1TV;
using RaceControl.Services.Interfaces.Github;
using RaceControl.ViewModels;
using RaceControl.Views;
using RaceControl.Vlc;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;
using LibVLCSharpCore = LibVLCSharp.Shared.Core;
using LogLevelNLog = NLog.LogLevel;
using LogLevelVLC = LibVLCSharp.Shared.LogLevel;

namespace RaceControl
{
    public partial class App
    {
        private readonly SplashScreen _splashScreen = new("splashscreen.png");

        protected override void OnStartup(StartupEventArgs e)
        {
            _splashScreen.Show(false);

            var currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (currentDirectory != null)
            {
                Environment.CurrentDirectory = currentDirectory;
            }

            base.OnStartup(e);
        }

        protected override void Initialize()
        {
            InitializeLogging();
            LibVLCSharpCore.Initialize();
            base.Initialize();
        }

        protected override void RegisterTypes(IContainerRegistry registry)
        {
            registry.RegisterDialogWindow<DialogWindow>();
            registry.RegisterDialogWindow<VideoDialogWindow>(nameof(VideoDialogWindow));
            registry.RegisterDialog<LoginDialog, LoginDialogViewModel>();
            registry.RegisterDialog<UpgradeDialog, UpgradeDialogViewModel>();
            registry.RegisterDialog<DownloadDialog, DownloadDialogViewModel>();
            registry.RegisterDialog<VideoDialog, VideoDialogViewModel>();

            registry
                .RegisterSingleton<IExtendedDialogService, ExtendedDialogService>()
                .RegisterSingleton<ISettings, Settings>()
                .RegisterSingleton<IVideoDialogLayout, VideoDialogLayout>()
                .RegisterInstance(CreateLibVLC())
                .Register<MediaPlayer>(CreateMediaPlayer)
                .Register<JsonSerializer>(() => new JsonSerializer { Formatting = Formatting.Indented })
                .Register<IAuthorizationService, AuthorizationService>()
                .Register<IApiService, ApiService>()
                .Register<IGithubService, GithubService>()
                .Register<ICredentialService, CredentialService>()
                .Register<INumberGenerator, NumberGenerator>()
                .Register<IDeviceLocator, DeviceLocator>()
                .Register<IMediaPlayer, VlcMediaPlayer>()
                .Register<IMediaDownloader, VlcMediaDownloader>();

            var container = registry.GetContainer();
            container.Register(Made.Of(() => CreateRestClient()), setup: Setup.With(asResolutionCall: true));
            container.Register(Made.Of<ILogger>(() => LogManager.GetLogger(Arg.Index<string>(0)), request => request.Parent.ImplementationType.Name));
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            _splashScreen.Close(TimeSpan.Zero);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            Container.Resolve<LibVLC>()?.Dispose();
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

        private static LibVLC CreateLibVLC()
        {
            var libVLC = new LibVLC();
            var logger = LogManager.GetLogger(libVLC.GetType().FullName);

            libVLC.Log += (_, args) =>
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

        private static MediaPlayer CreateMediaPlayer(IContainerProvider container)
        {
            return new(container.Resolve<LibVLC>())
            {
                EnableHardwareDecoding = true,
                EnableMouseInput = false,
                EnableKeyInput = false,
                FileCaching = 5000,
                NetworkCaching = 10000
            };
        }

        private static IRestClient CreateRestClient()
        {
            var restClient = new RestClient { UserAgent = nameof(RaceControl), ThrowOnAnyError = true };
            restClient.UseNewtonsoftJson();

            return restClient;
        }

        private void PrismApplication_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LogManager.GetCurrentClassLogger().Error(e.Exception, "An unhandled exception occurred.");
            MessageBoxHelper.ShowError(e.Exception.Message);
            e.Handled = true;
        }
    }
}