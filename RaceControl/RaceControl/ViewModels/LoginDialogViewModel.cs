using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json.Linq;
using System.Web;

namespace RaceControl.ViewModels;

public class LoginDialogViewModel : DialogViewModelBase
{
    private ICommand _sourceChangedCommand;

    public LoginDialogViewModel(ILogger logger) : base(logger)
    {
    }

    public override string Title => "Login";

    public ICommand SourceChangedCommand => _sourceChangedCommand ??= new DelegateCommand<WebView2>(SourceChangedExecute);

    private void SourceChangedExecute(WebView2 webView)
    {
        FindSubscriptionTokenAsync(webView).Await(LoginSuccess, LoginError, true);
    }

    private void LoginSuccess(string subscriptionToken)
    {
        if (!string.IsNullOrWhiteSpace(subscriptionToken))
        {
            Logger.Info("Login successful.");

            RaiseRequestClose(ButtonResult.OK, new DialogParameters
            {
                { ParameterNames.SubscriptionToken, subscriptionToken },
                { ParameterNames.SubscriptionStatus, "unknown" }
            });
        }
    }

    private void LoginError(Exception ex)
    {
        Logger.Error(ex, "Login error.");
    }

    private static async Task<string> FindSubscriptionTokenAsync(WebView2 webView)
    {
        var cookies = await webView.CoreWebView2.CookieManager.GetCookiesAsync(Constants.ApiEndpointUrl);
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