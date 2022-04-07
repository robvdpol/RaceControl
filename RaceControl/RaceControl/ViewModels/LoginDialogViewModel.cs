using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json.Linq;
using System.Web;

namespace RaceControl.ViewModels;

public class LoginDialogViewModel : DialogViewModelBase
{
    private readonly ICredentialService _credentialService;

    private ICommand _loginCommand;
    private ICommand _initializationCompletedCommand;
    private ICommand _navigationCompletedCommand;
    private ICommand _sourceChangedCommand;

    private string _email;
    private string _password;
    private WebView2 _webView;

    public LoginDialogViewModel(ILogger logger, ICredentialService credentialService) : base(logger)
    {
        _credentialService = credentialService;
    }

    public override string Title => "Login";

    public ICommand InitializationCompletedCommand => _initializationCompletedCommand ??= new DelegateCommand<WebView2>(InitializationCompletedExecute);
    public ICommand NavigationCompletedCommand => _navigationCompletedCommand ??= new DelegateCommand(NavigationCompletedExecute);
    public ICommand SourceChangedCommand => _sourceChangedCommand ??= new DelegateCommand(SourceChangedExecute);
    public ICommand LoginCommand => _loginCommand ??= new DelegateCommand(LoginExecute, CanLoginExecute).ObservesProperty(() => IsBusy).ObservesProperty(() => Email).ObservesProperty(() => Password);

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public override void OnDialogOpened(IDialogParameters parameters)
    {
        base.OnDialogOpened(parameters);

        if (_credentialService.LoadCredential(out var email, out var password))
        {
            Email = email;
            Password = password;
            LoginCommand.TryExecute();
        }
    }

    private void InitializationCompletedExecute(WebView2 webView)
    {
        _webView = webView;
        _webView.CoreWebView2.CookieManager.DeleteAllCookies();
    }

    private void NavigationCompletedExecute()
    {
        if (string.Equals(_webView.Source.ToString(), Constants.LoginUrl, StringComparison.OrdinalIgnoreCase))
        {
            SubmitLoginFormAsync().Await(() =>
            {
                IsBusy = false;
            });
        }
    }

    private void SourceChangedExecute()
    {
        FindSubscriptionTokenAsync().Await(LoginSuccess, LoginError, true);
    }

    private bool CanLoginExecute()
    {
        return !IsBusy && !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);
    }

    private void LoginExecute()
    {
        if (_webView == null)
        {
            return;
        }

        IsBusy = true;

        if (!string.Equals(_webView.Source.ToString(), Constants.LoginUrl, StringComparison.OrdinalIgnoreCase))
        {
            _webView.CoreWebView2.Navigate(Constants.LoginUrl);
        }
        else
        {
            _webView.Reload();
        }
    }

    private void LoginSuccess(string subscriptionToken)
    {
        if (string.IsNullOrWhiteSpace(subscriptionToken))
        {
            return;
        }

        Logger.Info("Login successful.");
        IsBusy = false;
        _credentialService.SaveCredential(Email, Password);

        RaiseRequestClose(ButtonResult.OK, new DialogParameters
        {
            { ParameterNames.SubscriptionToken, subscriptionToken },
            { ParameterNames.SubscriptionStatus, "active" }
        });
    }

    private void LoginError(Exception ex)
    {
        Logger.Warn(ex, "Login failed.");
        IsBusy = false;
    }

    private async Task SubmitLoginFormAsync()
    {
        await _webView.ExecuteScriptAsync("document.getElementById('truste-consent-button').click()");
        await Task.Delay(250);
        await _webView.ExecuteScriptAsync($"document.getElementsByName('Login')[0].value = '{Email}'");
        await _webView.ExecuteScriptAsync($"document.getElementsByName('Password')[0].value = '{Password}'");
        await Task.Delay(250);
        await _webView.ExecuteScriptAsync("document.querySelectorAll('button[type=submit]')[0].click()");
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