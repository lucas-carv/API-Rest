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
using NSE.Identidade.API.Data;
using System;
using System.Collections.Generic;
using System.Linq;
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
            * Configuration       -> Arquivo de configura��o
            * GetConnectionString -> Pega a connection string do Configuration
            */
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddDefaultIdentity<IdentityUser>()  // Configura��o padr�o do Identity
                    .AddRoles<IdentityRole>() // Suporte as Roles (regras), perfil de usu�rio, etc
                    .AddEntityFrameworkStores<ApplicationDbContext>() // Aplica��o do contexto para criar o banco, fazer leitura, etc
                    .AddDefaultTokenProviders(); // criptografia dentro de um 'link' para reconhecer quem est� acessando


            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //valida��o de ambiente de desenvolvimento
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage(); // utiliza��o das p�ginas de erros para desenvolvedor
            }

            app.UseHttpsRedirection(); // redirecionamento para protocolo HTTPS

            app.UseRouting(); // for�ando a utiliza��o de esquema de rotas

            app.UseAuthentication(); // utilizar a autentica��o
            app.UseAuthorization(); // utilizar autoriza��o 

            app.UseEndpoints(endpoints => // utiliza��o de endpoints das controllers
            {
                endpoints.MapControllers(); // mapeamento das controllers
            });
        }
    }
}
