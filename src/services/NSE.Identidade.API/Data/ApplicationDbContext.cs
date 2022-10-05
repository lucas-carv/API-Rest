using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace NSE.Identidade.API.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
         /* 
         * ApplicationDbContext Arquivo de contexto que herda do IdentityDbContext
         * IdentityDbContext classe do identity arquivo de contexto do próprio identity
         * o construtor recebe as options que são configurações passadas da StartUp
         */
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
    }
}
