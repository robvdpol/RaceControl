using CredentialManagement;
using RaceControl.Services.Interfaces.Credential;

namespace RaceControl.Services.Credential
{
    public class CredentialService : ICredentialService
    {
        private const string RaceControlF1TV = "RaceControlF1TV";

        public bool LoadCredential(out string username, out string password)
        {
            using (var credential = new CredentialManagement.Credential())
            {
                credential.Target = RaceControlF1TV;
                credential.Type = CredentialType.Generic;
                credential.PersistanceType = PersistanceType.LocalComputer;

                var loaded = credential.Load();

                if (loaded)
                {
                    username = credential.Username;
                    password = credential.Password;
                }
                else
                {
                    username = null;
                    password = null;
                }

                return loaded;
            }
        }

        public bool SaveCredential(string username, string password)
        {
            using (var credential = new CredentialManagement.Credential())
            {
                credential.Target = RaceControlF1TV;
                credential.Type = CredentialType.Generic;
                credential.PersistanceType = PersistanceType.LocalComputer;
                credential.Username = username;
                credential.Password = password;

                return credential.Save();
            }
        }

        public bool DeleteCredential()
        {
            using (var credential = new CredentialManagement.Credential())
            {
                credential.Target = RaceControlF1TV;
                credential.Type = CredentialType.Generic;
                credential.PersistanceType = PersistanceType.LocalComputer;

                return credential.Delete();
            }
        }
    }
}