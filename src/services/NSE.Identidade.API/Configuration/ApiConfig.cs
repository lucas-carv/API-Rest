using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace NSE.Identidade.API.Configuration
{
    public static class ApiConfig
    {
        public static IServiceCollection AddApiConfiguration(this IServiceCollection services)
        {
            services.AddControllers();
            return services;
        }

        public static IApplicationBuilder UseApiConfiguration(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            //Validação de ambiente de desenvolvimento
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage(); // Utilização das páginas de erros para desenvolvedor
            }

            app.UseHttpsRedirection();           // Redirecionamento para protocolo HTTPS

            app.UseRouting();                    // Forçando a utilização de esquema de rotas

            // O parametro abaixo DEVE ficar entre o Routing e os Endpoints
            app.UseIdentityConfiguration();

            app.UseEndpoints(endpoints =>        // utilização de endpoints das controllers
            {
                endpoints.MapControllers();      // mapeamento das controllers
            });

            return app;
        }
    }
}
