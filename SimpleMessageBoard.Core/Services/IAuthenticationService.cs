namespace SimpleMessageBoard.Services
{
    using SimpleMessageBoard.DTOs;
    using System.Threading.Tasks;

    public interface IUserService
    {
        Task<AuthToken> AuthenticateForToken(string userName, string password);
    }
}