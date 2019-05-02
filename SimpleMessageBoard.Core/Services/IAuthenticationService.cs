namespace SimpleMessageBoard.Services
{
    using SimpleMessageBoard.DTOs;
    using System.Threading.Tasks;

    public interface IAuthenticationService
    {
        Task<AuthToken> AuthenticateForToken(string userName, string password);
    }
}