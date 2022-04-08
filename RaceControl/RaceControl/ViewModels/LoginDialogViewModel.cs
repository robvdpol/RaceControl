using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json.Linq;
using System.Web;

namespace RaceControl.ViewModels;

public class LoginDialogViewModel : DialogViewModelBase
{
    private ICommand _initializationCompletedCommand;
    private ICommand _sourceChangedCommand;

    private WebView2 _webView;

    public LoginDialogViewModel(ILogger logger) : base(logger)
    {
    }

    public override string Title => "Login";

    public ICommand InitializationCompletedCommand => _initializationCompletedCommand ??= new DelegateCommand<WebView2>(InitializationCompletedExecute);
    public ICommand SourceChangedCommand => _sourceChangedCommand ??= new DelegateCommand(SourceChangedExecute);

    private void InitializationCompletedExecute(WebView2 webView)
    {
        _webView = webView;
        _webView.CoreWebView2.CookieManager.DeleteAllCookies();
        _webView.Source = new Uri("https://account.formula1.com/#/en/login");
    }

    private void SourceChangedExecute()
    {
        FindSubscriptionTokenAsync().Await(LoginSuccess, LoginError, true);
    }

    private void LoginSuccess(string subscriptionToken)
    {
        if (string.IsNullOrWhiteSpace(subscriptionToken))
        {
            return;
        }

        Logger.Info("Login successful.");

        RaiseRequestClose(ButtonResult.OK, new DialogParameters
        {
            { ParameterNames.SubscriptionToken, subscriptionToken },
            { ParameterNames.SubscriptionStatus, "unknown" }
        });
    }

    private void LoginError(Exception ex)
    {
        Logger.Error(ex, "Login error.");
    }

    private async Task<string> FindSubscriptionTokenAsync()
    {
        var cookies = await _webView.CoreWebView2.CookieManager.GetCookiesAsync(Constants.ApiEndpointUrl);
        var loginCookie = cookies.FirstOrDefault(cookie => string.Equals(cookie.Name, "login-session", StringComparison.OrdinalIgnoreCase));

        if (loginCookie != null)
        {
            var loginJson = HttpUtility.UrlDecode(loginCookie.Value);
            var loginSession = JsonConvert.DeserializeObject<JObject>(loginJson);
            var subscriptionToken = loginSession?["data"]?["subscriptionToken"];

            if (subscriptionToken != null)
            {
                return subscriptionToken.Value<string>();
            }
        }

        return null;
    }
}