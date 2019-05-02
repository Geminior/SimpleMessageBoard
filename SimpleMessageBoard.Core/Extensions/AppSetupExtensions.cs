namespace SimpleMessageBoard
{
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.IdentityModel.Tokens;
    using SimpleMessageBoard.Configuration;
    using SimpleMessageBoard.DAL;
    using SimpleMessageBoard.Model;
    using SimpleMessageBoard.Services;

    public static class AppSetupExtensions
    {
        public static IServiceCollection AddMessageBoard(this IServiceCollection services, IConfiguration configuration)
        {
            //Setup config options
            services.ConfigureSection<TokensConfig>(configuration);

            //Setup data store
            services.AddDbContext<MessageBoardDbContext>(options => options.UseInMemoryDatabase("MessageBoardDb"));

            //Configure authentication
            services.AddSingleton<IPasswordHasher<BoardUser>, PasswordHasher<BoardUser>>();

            var tokenCfg = configuration.GetSection<TokensConfig>();
            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opt =>
            {
                opt.SaveToken = true;
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(tokenCfg.SecretBytes),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            //Configure app services
            services.AddScoped<ITokenIssuer, TokenIssuer>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IMessageBoardService, MessageBoardService>();

            return services;
        }
    }
}
