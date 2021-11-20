using DryIoc;
using FlyleafLib;
using FlyleafLib.MediaFramework.MediaContext;
using FlyleafLib.MediaPlayer;
using GoogleCast;
using Newtonsoft.Json;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using Prism.DryIoc;
using Prism.Ioc;
using RaceControl.Common.Generators;
using RaceControl.Common.Utils;
using RaceControl.Core.Helpers;
using RaceControl.Core.Settings;
using RaceControl.Extensions;
using RaceControl.Flyleaf;
using RaceControl.Interfaces;
using RaceControl.Services.Credential;
using RaceControl.Services.F1TV;
using RaceControl.Services.Github;
using RaceControl.Services.Interfaces.Credential;
using RaceControl.Services.Interfaces.F1TV;
using RaceControl.Services.Interfaces.Github;
using RaceControl.ViewModels;
using RaceControl.Views;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace RaceControl
{
    public partial class App
    {
        private readonly SplashScreen _splashScreen = new("splashscreen.png");

        private static int _flyleafUniqueId;

        protected override void OnStartup(StartupEventArgs e)
        {
            _splashScreen.Show(false, true);

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

            var path = Path.Combine(Environment.CurrentDirectory, "FFmpeg");
            Master.RegisterFFmpeg(path);

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
                .Register<Player>(CreateFlyleafPlayer)
                .Register<Downloader>(CreateFlyleafDownloader)
                .Register<JsonSerializer>(() => new JsonSerializer { Formatting = Formatting.Indented })
                .Register<IAuthorizationService, AuthorizationService>()
                .Register<IApiService, ApiService>()
                .Register<IGithubService, GithubService>()
                .Register<ICredentialService, CredentialService>()
                .Register<INumberGenerator, NumberGenerator>()
                .Register<IDeviceLocator, DeviceLocator>()
                .Register<ISender>(() => new Sender())
                .Register<IMediaPlayer, FlyleafMediaPlayer>()
                .Register<IMediaDownloader, FlyleafMediaDownloader>();

            var container = registry.GetContainer();
            container.Register(Made.Of(() => CreateRestClient()), setup: Setup.With(asResolutionCall: true));
            container.Register(Made.Of<ILogger>(() => LogManager.GetLogger(Arg.Index<string>(0)), request => request.Parent.ImplementationType.Name));
        }

        protected override Window CreateShell()
        {
            _splashScreen.Close(TimeSpan.Zero);

            return Container.Resolve<MainWindow>();
        }

        private static void InitializeLogging()
        {
            var config = new LoggingConfiguration();
            var logfile = new FileTarget("logfile")
            {
                FileName = FolderUtils.GetLogFilePath(),
                Layout = Layout.FromString("${longdate} ${uppercase:${level}} ${message}${onexception:inner=${newline}${exception:format=tostring}}"),
                ArchiveAboveSize = 1024 * 1024,
                ArchiveNumbering = ArchiveNumberingMode.Rolling,
                MaxArchiveFiles = 2
            };
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logfile);
            LogManager.Configuration = config;
        }

        private static Player CreateFlyleafPlayer()
        {
            var config = new Config();
            config.Demuxer.FormatOpt.Add("probesize", (50 * 1024 * 1024).ToString());
            config.Demuxer.FormatOpt.Add("analyzeduration", (10 * 1000 * 1000).ToString());
            config.Demuxer.ReadTimeout = TimeSpan.FromSeconds(30).Ticks;
            config.Demuxer.BufferDuration = TimeSpan.FromSeconds(1).Ticks;

            return new(config);
        }

        private static Downloader CreateFlyleafDownloader()
        {
            var config = new Config();
            config.Demuxer.FormatOpt.Add("probesize", (50 * 1024 * 1024).ToString());
            config.Demuxer.FormatOpt.Add("analyzeduration", (10 * 1000 * 1000).ToString());
            config.Demuxer.ReadTimeout = TimeSpan.FromSeconds(30).Ticks;

            return new Downloader(config, ++_flyleafUniqueId);
        }

        private static IRestClient CreateRestClient()
        {
            var restClient = new RestClient
            {
                UserAgent = nameof(RaceControl),
                Timeout = 30000,
                ReadWriteTimeout = 60000,
                ThrowOnAnyError = true
            };
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