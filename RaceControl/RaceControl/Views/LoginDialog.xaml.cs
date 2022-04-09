using Microsoft.Web.WebView2.Core;

namespace RaceControl.Views;

public partial class LoginDialog
{
    public LoginDialog()
    {
        InitializeComponent();
    }

    private void LoginDialogLoaded(object sender, RoutedEventArgs e)
    {
        InitializeWebViewAsync().Await();
    }

    private async Task InitializeWebViewAsync()
    {
        var userDataFolder = FolderUtils.GetWebView2UserDataPath();
        var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: userDataFolder);
        await WebView2.EnsureCoreWebView2Async(environment);
        WebView2.CoreWebView2.CookieManager.DeleteAllCookies();
        WebView2.Source = new Uri("https://account.formula1.com/#/en/login");
    }
}