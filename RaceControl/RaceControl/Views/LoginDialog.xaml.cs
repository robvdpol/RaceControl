using Microsoft.Web.WebView2.Core;

namespace RaceControl.Views;

public partial class LoginDialog
{
    private readonly ILogger _logger;

    public LoginDialog(ILogger logger)
    {
        InitializeComponent();
        _logger = logger;
    }

    private void LoginDialogLoaded(object sender, RoutedEventArgs e)
    {
        InitializeWebViewAsync().Await(InitializeWebViewSuccess, InitializeWebViewFailed, true);
    }

    private async Task InitializeWebViewAsync()
    {
        var userDataFolder = FolderUtils.GetWebView2UserDataPath();
        var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: userDataFolder);
        await WebView2.EnsureCoreWebView2Async(environment);
    }

    private void InitializeWebViewSuccess()
    {
        WebView2.CoreWebView2.CookieManager.DeleteAllCookies();
        WebView2.Source = new Uri("https://account.formula1.com/#/en/login");
    }

    private void InitializeWebViewFailed(Exception ex)
    {
        _logger.Error(ex, "An error occurred while initializing webview.");
    }
}