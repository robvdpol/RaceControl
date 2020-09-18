using RestSharp;
using RestSharp.Authenticators;

namespace RaceControl.Services.Lark
{
    public class F1TVAuthenticator : IAuthenticator
    {
        private readonly string _token;

        public F1TVAuthenticator(string token)
        {
            _token = token;
        }

        public void Authenticate(IRestClient client, IRestRequest request)
        {
            request.AddHeader("Authorization", $"JWT {_token}");
        }
    }
}