using DryIoc;
using GoogleCast;
using Prism.DryIoc;
using Prism.Ioc;
using RaceControl.Common.JsonConverter;
using RaceControl.ViewModels;
using RestSharp;
using RestSharp.Serializers.Json;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Threading;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace RaceControl;

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
        InitializeFlyleaf();

        base.Initialize();
    }

    protected override void RegisterTypes(IContainerRegistry registry)
    {
        registry.RegisterDialogWindow<Views.DialogWindow>();
        registry.RegisterDialogWindow<VideoDialogWindow>(nameof(VideoDialogWindow));
        registry.RegisterDialog<LoginDialog, LoginDialogViewModel>();
        registry.RegisterDialog<UpgradeDialog, UpgradeDialogViewModel>();
        registry.RegisterDialog<DownloadDialog, DownloadDialogViewModel>();
        registry.RegisterDialog<VideoDialog, VideoDialogViewModel>();

        registry
            .RegisterSingleton<IExtendedDialogService, ExtendedDialogService>()
            .RegisterSingleton<ISettings, Settings>()
            .RegisterSingleton<IVideoDialogLayout, VideoDialogLayout>()
            .RegisterSingleton<IApiService, ApiService>()
            .RegisterSingleton<IGithubService, GithubService>()
            .Register<Player>(CreateFlyleafPlayer)
            .Register<Downloader>(CreateFlyleafDownloader)
            .Register<RestClient>(CreateRestClient)
            .Register<JsonSerializer>(() => new JsonSerializer { Formatting = Formatting.Indented })
            .Register<IMediaPlayer, FlyleafMediaPlayer>()
            .Register<IMediaDownloader, FlyleafMediaDownloader>()
            .Register<INumberGenerator, NumberGenerator>()
            .Register<IDeviceLocator, DeviceLocator>()
            .Register<ISender>(() => new Sender());

        var container = registry.GetContainer();
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
            FileName = FolderUtils.GetApplicationLogFilePath(),
            Layout = Layout.FromString("${longdate} ${uppercase:${level}} ${message}${onexception:inner=${newline}${exception:format=tostring}}"),
            ArchiveAboveSize = 1024 * 1024,
            ArchiveNumbering = ArchiveNumberingMode.Rolling,
            MaxArchiveFiles = 2
        };
        config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logfile);
        LogManager.Configuration = config;
    }

    private static void InitializeFlyleaf()
    {
        Engine.Start(new EngineConfig
        {
            FFmpegPath = Path.Combine(Environment.CurrentDirectory, "FFmpeg"),
            //LogOutput = FolderUtils.GetFlyleafLogFilePath(),
            LogLevel = FlyleafLib.LogLevel.Debug,
            FFmpegLogLevel = FFmpegLogLevel.Warning
        });
    }

    private static Player CreateFlyleafPlayer()
    {
        var config = new Config();
        config.Player.KeyBindings.Enabled = false;
        config.Player.MouseBindings.Enabled = false;
        config.Player.AutoPlay = false;
        config.Demuxer.ReadTimeout = TimeSpan.FromSeconds(30).Ticks;
        config.Demuxer.BufferDuration = TimeSpan.FromSeconds(1).Ticks;
        config.Decoder.AllowProfileMismatch = false;

        return new Player(config);
    }

    private static Downloader CreateFlyleafDownloader()
    {
        var config = new Config
        {
            Demuxer =
                {
                    ReadTimeout = TimeSpan.FromSeconds(30).Ticks
                }
        };

        return new Downloader(config, ++_flyleafUniqueId);
    }

    private static RestClient CreateRestClient()
    {
        var restClientOptions = new RestClientOptions
        {
            ThrowOnAnyError = true,
            Timeout = 30000,
            UserAgent = nameof(RaceControl)
        };

        var restClient = new RestClient(restClientOptions);

        var options = new JsonSerializerOptions()
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        options.Converters.Add(new NullableDateTimeConverter());
        options.Converters.Add(new NullableIntConverter());
        restClient.UseSystemTextJson(options);

        return restClient;
    }

    private void PrismApplication_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        LogManager.GetCurrentClassLogger().Error(e.Exception, "An unhandled exception occurred.");
        MessageBoxHelper.ShowError(e.Exception.Message);
        e.Handled = true;
    }
}