namespace SimpleMessageBoard.Services
{
    using SimpleMessageBoard.DTOs;

    public interface ITokenIssuer
    {
        AuthToken IssueToken(string userId);
    }
}