namespace SimpleMessageBoard.Services
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;
    using SimpleMessageBoard.Configuration;
    using SimpleMessageBoard.DTOs;
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;

    public sealed class TokenIssuer : ITokenIssuer
    {
        private TokensConfig _cfg;
        private ILogger _logger;

        public TokenIssuer(IOptions<TokensConfig> tokenCfg, ILogger<TokenIssuer> logger)
        {
            if (tokenCfg == null || tokenCfg.Value == null)
            {
                throw new ArgumentNullException(nameof(tokenCfg));
            }

            _cfg = tokenCfg.Value;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public AuthToken IssueToken(string userId)
        {
            _logger.LogInformation("[S] Issuing token to user: " + userId);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, userId)
                }),
                Expires = _cfg.TokenDuration.HasValue ? DateTime.Now.Add(_cfg.TokenDuration.Value) : default(DateTime?),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(_cfg.SecretBytes), SecurityAlgorithms.HmacSha256Signature)
            };

            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(tokenDescriptor);
            var tokenString = handler.WriteToken(token);

            _logger.LogInformation("[E] Issued token to user: " + userId);
            return new AuthToken { Value = tokenString };
        }
    }
}
