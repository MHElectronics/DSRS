using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Claims;
using BOL;

namespace AxleLoadSystem.Authentication;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ProtectedLocalStorage _sessionStorage;
    private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

    public CustomAuthenticationStateProvider(ProtectedLocalStorage sessionStorage)
    {
        _sessionStorage = sessionStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var userSessonStorageResult = await _sessionStorage.GetAsync<User>("UserSession");
            var userSession = userSessonStorageResult.Success ? userSessonStorageResult.Value : null;

            if (userSession == null)
            {
                return await Task.FromResult(new AuthenticationState(_anonymous));
            }

            //add claims
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userSession.Id.ToString())
                ,new Claim(ClaimTypes.Name, userSession.Name)
            };

            string[] roles = userSession.Role.Split(',');
            foreach (string role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Trim()));
            }

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "CustomAuth"));

            //var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>{
            //    new Claim(ClaimTypes.NameIdentifier, userSession.Id.ToString())
            //    ,new Claim(ClaimTypes.Name, userSession.Name)
            //    //,new Claim(ClaimTypes.Role, userSession.Role)
            //}, "CustomAuth"));

            return await Task.FromResult(new AuthenticationState(claimsPrincipal));
        }
        catch
        {
            return await Task.FromResult(new AuthenticationState(_anonymous));
        }
    }

    public async Task UpdateAuthenticationState(User? user)
    {
        ClaimsPrincipal claimsPrincipal;
        if (user != null)
        {
            await _sessionStorage.SetAsync("UserSession", new { user.Id, user.Name, user.Email, user.Role });

            //add claims
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                ,new Claim(ClaimTypes.Name, user.Name)
            };

            string[] roles = user.Role.Split(',');
            foreach (string role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Trim()));
            }

            claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "CustomAuth"));

            //claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>{
            //    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            //    new Claim(ClaimTypes.Name, user.Name),
            //    new Claim(ClaimTypes.Role, user.Role)
            //}, "CustomAuth"));
        }
        else
        {
            await _sessionStorage.DeleteAsync("UserSession");
            claimsPrincipal = _anonymous;
        }

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
    }

    public async Task<User?> GetAuthorizedUser()
    {
        var userSessonStorageResult = await _sessionStorage.GetAsync<User>("UserSession");
        User? user = userSessonStorageResult.Success ? userSessonStorageResult.Value : null;
        return user;
    }
}
