namespace SimpleMessageBoard
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using SimpleMessageBoard.Configuration;
    using SimpleMessageBoard.DAL;
    using SimpleMessageBoard.Model;
    using SimpleMessageBoard.Services;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //Setup config options
            services.ConfigureSection<TokensConfig>(this.Configuration);

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddDbContext<MessageBoardDbContext>(opt => opt.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=SimpleMessageBoardDb;Trusted_Connection=True;MultipleActiveResultSets=true"));
            //options => options.UseInMemoryDatabase("MessageBoardDb"));
            //Configure auth stuff
            services.AddSingleton<IPasswordHasher<BoardUser>, PasswordHasher<BoardUser>>();

            //Configure app services
            services.AddScoped<ITokenIssuer, TokenIssuer>();
            services.AddScoped<IUserService, UserService>();
            //services.AddScoped<ITokenIssuer, TokenIssuer>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
