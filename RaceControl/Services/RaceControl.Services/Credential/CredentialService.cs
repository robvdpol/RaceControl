using CredentialManagement;
using NLog;
using RaceControl.Services.Interfaces.Credential;

namespace RaceControl.Services.Credential
{
    public class CredentialService : ICredentialService
    {
        private const string RaceControlF1TV = "RaceControlF1TV";

        private readonly ILogger _logger;

        public CredentialService(ILogger logger)
        {
            _logger = logger;
        }

        public bool LoadCredential(out string username, out string password)
        {
            _logger.Info("Loading credentials from store...");

            using var credential = new CredentialManagement.Credential
            {
                Target = RaceControlF1TV,
                Type = CredentialType.Generic,
                PersistanceType = PersistanceType.LocalComputer
            };

            var loaded = credential.Load();

            if (loaded)
            {
                username = credential.Username;
                password = credential.Password;
                _logger.Info("Credentials loaded from store.");
            }
            else
            {
                username = null;
                password = null;
                _logger.Warn("Credentials not found in store.");
            }

            return loaded;
        }

        public void SaveCredential(string username, string password)
        {
            _logger.Info("Saving credentials to store...");

            using var credential = new CredentialManagement.Credential
            {
                Target = RaceControlF1TV,
                Type = CredentialType.Generic,
                PersistanceType = PersistanceType.LocalComputer,
                Username = username,
                Password = password
            };

            if (credential.Save())
            {
                _logger.Info("Credentials saved to store.");
            }
            else
            {
                _logger.Warn("Credentials not saved to store.");
            }
        }

        public void DeleteCredential()
        {
            _logger.Info("Deleting credentials from store...");

            using var credential = new CredentialManagement.Credential
            {
                Target = RaceControlF1TV,
                Type = CredentialType.Generic,
                PersistanceType = PersistanceType.LocalComputer
            };

            if (credential.Delete())
            {
                _logger.Info("Credentials deleted from store.");
            }
            else
            {
                _logger.Warn("Credentials not deleted from store.");
            }
        }
    }
}