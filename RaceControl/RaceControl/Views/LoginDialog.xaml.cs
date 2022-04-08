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
        var userDataFolder = FolderUtils.GetWebView2UserDataPath();

        CoreWebView2Environment.CreateAsync(userDataFolder: userDataFolder).Await(environment =>
        {
            WebView2.EnsureCoreWebView2Async(environment).Await();
        });
    }
}