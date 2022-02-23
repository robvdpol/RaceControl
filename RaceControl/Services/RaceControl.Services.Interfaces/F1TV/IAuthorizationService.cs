using RaceControl.Services.Interfaces.F1TV.Authorization;

namespace RaceControl.Services.Interfaces.F1TV;

public interface IAuthorizationService
{
    Task<AuthResponse> AuthenticateAsync(string login, string password);
}
