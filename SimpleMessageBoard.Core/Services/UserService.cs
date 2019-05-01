namespace SimpleMessageBoard.Services
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using SimpleMessageBoard.DAL;
    using SimpleMessageBoard.DTOs;
    using SimpleMessageBoard.Model;
    using System;
    using System.Threading.Tasks;

    public sealed class UserService : IUserService
    {
        private ITokenIssuer _tokenIssuer;
        private IPasswordHasher<BoardUser> _passHasher;
        private MessageBoardDbContext _ctx;
        private ILogger _logger;

        public UserService(MessageBoardDbContext ctx, IPasswordHasher<BoardUser> passHasher, ITokenIssuer tokenIssuer, ILogger<UserService> logger)
        {
            _ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
            _passHasher = passHasher ?? throw new ArgumentNullException(nameof(passHasher));
            _tokenIssuer = tokenIssuer ?? throw new ArgumentNullException(nameof(tokenIssuer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AuthToken> AuthenticateForToken(string userName, string password)
        {
            _logger.LogInformation("[S] Authentication for {UserName}", userName);

            var user = await _ctx.Users.FirstOrDefaultAsync(u => u.UserName == userName); //TODO: Case sensitivity issue

            if (user == null)
            {
                //For ease of use, as this is not the central part of the demo. Doing user registration this way in a live app would be problematic.
                user = new BoardUser
                {
                    UserName = userName
                };

                user.Password = _passHasher.HashPassword(user, password);

                _ctx.Users.Add(user);

                try
                {
                    await _ctx.SaveChangesAsync();
                    _logger.LogInformation("User {UserName} created.", userName);
                }
                catch (DbUpdateException dbex)
                {
                    //Most likely cause would be index violation
                    _logger.LogWarning(dbex, "[E] Failed to create user {UserName}.", userName);
                    return null;
                }
            }
            else if (_passHasher.VerifyHashedPassword(user, user.Password, password) == PasswordVerificationResult.Failed)
            {
                _logger.LogInformation("[E] Authentication for {UserName} failed.", userName);
                return null;
            }

            _logger.LogInformation("[E] Authentication for {UserName} succeeded.", userName);
            return _tokenIssuer.IssueToken(user.Id.ToString());
        }
    }
}
