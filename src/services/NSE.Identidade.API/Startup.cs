using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSE.Identidade.API.Configuration;

namespace NSE.Identidade.API
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IHostEnvironment hostEnvironment)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(hostEnvironment.ContentRootPath)                                   // Pega o caminho onde a aplica��o est� com base no hostEnvironment
                .AddJsonFile("appsettings.json", true, true)                                    // Registra o arquivo appSettings.Json dentro das configura��es
                .AddJsonFile($"appsettings.{hostEnvironment.EnvironmentName}.json", true, true) // Caso levante a API em ambiente de desenvolvimento, vai ler o arquivo appSettingsDevelopment.json
                .AddEnvironmentVariables();

            if (hostEnvironment.IsDevelopment())
            {
                //builder.AddUserSecrets<Startup>();
            }

            Configuration = builder.Build(); // Leva as configura��es do builder para o Configuration global da Startup
        }


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddIdentityConfiguration(Configuration);
            services.AddApiConfiguration();
            services.AddSwaggerConfiguration();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwaggerConfiguration();
            app.UseApiConfiguration(env);
        }
    }
}
