using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NSE.Identidade.API.Data;
using NSE.Identidade.API.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSE.Identidade.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            /* 
            * AddDbContext        -> adiciona um contexto do Entity Framework (ApplicationDbContext)
            * options             -> UseSqlServer, utiliza o suporte ao Sql Server
            * Configuration       -> Arquivo de configuração
            * GetConnectionString -> Pega a connection string do Configuration
            */
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddDefaultIdentity<IdentityUser>()               // Configuração padrão do Identity
                    .AddRoles<IdentityRole>()                         // Suporte as Roles (regras), perfil de usuário, etc
                    .AddEntityFrameworkStores<ApplicationDbContext>() // Aplicação do contexto para criar o banco, fazer leitura, etc
                    .AddDefaultTokenProviders();                      // criptografia dentro de um 'link' para reconhecer quem está acessando

            var appSettingsSection = Configuration.GetSection("AppSettings");
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
                bearerOptions.SaveToken = true; // Salva o token
                bearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = appSettings.ValidoEm,
                    ValidIssuer = appSettings.Emissor
                };
            })

                ;


            services.AddControllers();


            services.AddSwaggerGen(c =>  //Middler do Swagger para o PipeLine
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "NerdStore",
                    Description = "API do curso do Desenvolvedor IO",
                    Contact = new OpenApiContact() { Name = "Lucas Carvalho", Email = "lucas.carvalho10797@gmail.com" }
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            });

            //validação de ambiente de desenvolvimento
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage(); // utilização das páginas de erros para desenvolvedor
            }

            app.UseHttpsRedirection(); // redirecionamento para protocolo HTTPS

            app.UseRouting(); // forçando a utilização de esquema de rotas

            app.UseAuthentication(); // utilizar a autenticação
            app.UseAuthorization(); // utilizar autorização 

            app.UseEndpoints(endpoints => // utilização de endpoints das controllers
            {
                endpoints.MapControllers(); // mapeamento das controllers
            });
        }
    }
}
