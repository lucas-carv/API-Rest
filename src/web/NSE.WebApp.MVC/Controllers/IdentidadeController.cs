using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using NSE.WebApp.MVC.Models;
using NSE.WebApp.MVC.Services;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NSE.WebApp.MVC.Controllers
{
    public class IdentidadeController : Controller
    {
        private readonly IAutenticacaoService _autenticacaoService;

        public IdentidadeController(IAutenticacaoService autenticacaoService)
        {
            _autenticacaoService = autenticacaoService;
        }

        [HttpGet]
        [Route("nova-conta")]
        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        [Route("nova-conta")]
        public async Task<IActionResult> Registro(UsuarioRegistro usuarioRegistro)
        {
            if (!ModelState.IsValid)
                return View(usuarioRegistro);

            // API - Registro
            var resposta = await _autenticacaoService.Registro(usuarioRegistro);

            //if (false) return View(usuarioRegistro);

            // Realizar login na APP
            await RealizarLogin(resposta);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Route("login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost()]
        [Route("login")]
        public async Task<IActionResult> Login(UsuarioLogin usuarioLogin)
        {

            if (!ModelState.IsValid)
                return View(usuarioLogin);

            // API - Login

            var resposta = await _autenticacaoService.Login(usuarioLogin);

            //if (false) return View(usuarioLogin);

            // Realizar login na APP
            await RealizarLogin(resposta);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Route("sair")]
        public async Task<IActionResult> Logout()
        {
            return RedirectToAction("Index", "Home");
        }

        private async Task RealizarLogin(UsuarioRespostaLogin resposta)
        {
            var token = ObterTokenFormatado(resposta.AccessToken);

            var claims = new List<Claim>();
            claims.Add(new Claim("JWT", resposta.AccessToken));
            claims.AddRange(token.Claims);

            var claimsIdentity = new ClaimsIdentity(                      // Gera as Claims dentro do Cookie
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                ExpiresUtc = System.DateTimeOffset.UtcNow.AddMinutes(60), // Tempo de expiração da autenticação do Cookie
                IsPersistent = true                                       // Configuração para fazer com que o Cookie dure por vários requests
            };

            await HttpContext.SignInAsync(                                // Método de login do HttpContext
                CookieAuthenticationDefaults.AuthenticationScheme,        // Parâmetro para dizer ao http que vai trabalhar com o tipo de autenticação com Cookies
                new ClaimsPrincipal(claimsIdentity),                      // Todas as claims do usuário, inclusive o JWT
                authProperties);                                          // As configurações da autenticação
        }

        private static JwtSecurityToken ObterTokenFormatado(string jwtToken)
        {
            return new JwtSecurityTokenHandler().ReadToken(jwtToken) as JwtSecurityToken;
        }
    }
}
