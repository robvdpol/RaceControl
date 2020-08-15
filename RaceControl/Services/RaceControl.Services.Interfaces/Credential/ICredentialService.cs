namespace RaceControl.Services.Interfaces.Credential
{
    public interface ICredentialService
    {
        bool LoadCredential(out string username, out string password);

        bool SaveCredential(string username, string password);

        bool DeleteCredential();
    }
}