using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NSE.Identidade.API.Models;
using System.Threading.Tasks;

namespace NSE.Identidade.API.Controllers
{
    [Route("api/identidade")]
    public class AuthController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager; // Gerenciador de login
        private readonly UserManager<IdentityUser> _userManager; // Propriedade para administrar o usuário

        public AuthController(SignInManager<IdentityUser> signInManager,
                              UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpPost("nova-conta")]
        public async Task<ActionResult> Registrar(UsuarioRegistro usuarioRegistro)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = new IdentityUser
            {
                UserName = usuarioRegistro.Email,
                Email = usuarioRegistro.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, usuarioRegistro.Senha); // Criando o usuário, a senha é passada como parâmetro para criptografia
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, false); // Método para logar no sistema
                return Ok();
            }

            return BadRequest();
        }

        [HttpPost("autenticar")]
        public async Task<ActionResult> Login(UsuarioRegistro usuarioLogin)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var result = await _signInManager.PasswordSignInAsync(usuarioLogin.Email,
                usuarioLogin.Senha,
                false, // Grava as informações do usuário que está logando
                true); // bloqueia o usuário por 5 minutos se ficar errando a senha

            if (result.Succeeded)
                return Ok();

            return BadRequest();
        }
    }
}
