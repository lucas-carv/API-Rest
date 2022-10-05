using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NSE.Identidade.API.Data;
using NSE.Identidade.API.Extensions;
using System.Runtime.CompilerServices;
using System.Text;

namespace NSE.Identidade.API.Configuration
{
    public static class IdentityConfig
    {
        public static IServiceCollection AddIdentityConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            /* 
            * AddDbContext        -> Adiciona um contexto do Entity Framework (ApplicationDbContext)
            * options             -> UseSqlServer, utiliza o suporte ao Sql Server
            * Configuration       -> Arquivo de configuração
            * GetConnectionString -> Pega a connection string do Configuration
            */
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddDefaultIdentity<IdentityUser>()               // Configuração padrão do Identity
                    .AddRoles<IdentityRole>()                         // Suporte as Roles (regras), perfil de usuário, etc
                    .AddErrorDescriber<IdentityMensagensPortugues>()  // Describer que vai trocar as mensagens de erros do Identity
                    .AddEntityFrameworkStores<ApplicationDbContext>() // Aplicação do contexto para criar o banco, fazer leitura, etc
                    .AddDefaultTokenProviders();                      // criptografia dentro de um 'link' para reconhecer quem está acessando


            // JWT

            var appSettingsSection = configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);

            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);


            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // Formas de autenticação, no caso utilizando o AuthenticationScheme do JWT
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(bearerOptions =>
            {
                bearerOptions.RequireHttpsMetadata = true;  // Requer acesso pelo HTTPS
                bearerOptions.SaveToken = true;             // Salva o token
                bearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = appSettings.ValidoEm,
                    ValidIssuer = appSettings.Emissor
                };
            });

            return services;
        }

        public static IApplicationBuilder UseIdentityConfiguration(this IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseAuthorization();
            return app;
        }
    }
}
