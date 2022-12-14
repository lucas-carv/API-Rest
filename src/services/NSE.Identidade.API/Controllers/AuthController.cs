using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NSE.Identidade.API.Extensions;
using NSE.Identidade.API.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace NSE.Identidade.API.Controllers
{

    [Route("api/identidade")]
    public class AuthController : MainController
    {
        private readonly SignInManager<IdentityUser> _signInManager; // Gerenciador de login
        private readonly UserManager<IdentityUser> _userManager;     // Propriedade para administrar o usuário
        private readonly AppSettings _appSettings;

        public AuthController(SignInManager<IdentityUser> signInManager,
                              UserManager<IdentityUser> userManager,
                              IOptions<AppSettings> appSettings) // IOptions serve para ler arquivos de configuração que no caso vai ser o AppSettingsDevelopment
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _appSettings = appSettings.Value;
        }

        [HttpPost("nova-conta")]
        public async Task<ActionResult> Registrar(UsuarioRegistro usuarioRegistro)
        {
            if (!ModelState.IsValid)
                return CustomResponse();

            var user = new IdentityUser
            {
                UserName = usuarioRegistro.Email,
                Email = usuarioRegistro.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, usuarioRegistro.Senha); // Criando o usuário, a senha é passada como parâmetro para criptografia

            if (result.Succeeded)
                return CustomResponse(await GerarJwt(usuarioRegistro.Email));

            foreach (var error in result.Errors)
            {
                AdicionarErroProcessamento(error.Description);
            }

            return CustomResponse();
        }

        [HttpPost("autenticar")]
        public async Task<ActionResult> Login(UsuarioLogin usuarioLogin)
        {
            if (!ModelState.IsValid)
                return CustomResponse();

            var result = await _signInManager.PasswordSignInAsync(usuarioLogin.Email,
                                                                  usuarioLogin.Senha,
                                                                  false, // Grava as informações do usuário que está logando
                                                                  true); // bloqueia o usuário por 5 minutos se ficar errando a senha

            if (result.Succeeded)
                return CustomResponse(await GerarJwt(usuarioLogin.Email));

            if (result.IsLockedOut)
            {
                AdicionarErroProcessamento("Usuário temporiamente bloqueado por tentativas inválidas");
                return CustomResponse();
            }

            AdicionarErroProcessamento("Usuário ou Senha incorretos");
            return CustomResponse();
        }


        private async Task<UsuarioRespostaLogin> GerarJwt(string email)
        {
            IdentityUser user = await _userManager.FindByEmailAsync(email);         // Identifica o usuário pelo email logado
            IList<Claim> claims = await _userManager.GetClaimsAsync(user);          // Nesse ponto cria as Claims vazias

            ClaimsIdentity identityClaims = await ObterClaimsUsuario(claims, user); // Método para anexar Claims ao Usuário logado
            string encodedToken = CodificarToken(identityClaims);                   // Gera o token codificado do Usuário e as Claims dele

            return ObterRespostaToken(encodedToken, user, claims);                  // Retorna o UsuarioRespostaLogin que armazena as informações do JWT e o usuário logado
        }

        private async Task<ClaimsIdentity> ObterClaimsUsuario(ICollection<Claim> claims, IdentityUser user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            //  RFC //
            // - JTI - Fornece um id exclusivo para o JWT, para evitar que um valor seja atribuido a objetos de dados diferentes, evita uma chance 'despresível' de colisões de emissores
            // - NBF - Identifica o tempo do JWT, para que NÃO seja aceito processamento anterior ao tempo gerado pelo JWT
            // - IAT - Identifica o momento em que o JWT foi gerado e publicado, pode ser usado para determinar a idade do JWT
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Id));
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, ToUnixEpochDate(DateTime.UtcNow).ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(DateTime.UtcNow).ToString(), ClaimValueTypes.Integer64));
            foreach (var userRole in userRoles)
            {
                claims.Add(new Claim("role", userRole));
            }

            var identityClaims = new ClaimsIdentity();
            identityClaims.AddClaims(claims);

            return identityClaims;
        }

        private string CodificarToken(ClaimsIdentity identityClaims)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = _appSettings.Emissor,
                Audience = _appSettings.ValidoEm,
                Subject = identityClaims,
                Expires = DateTime.UtcNow.AddHours(_appSettings.ExpiracaoHoras),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            });

            return tokenHandler.WriteToken(token);
        }

        private UsuarioRespostaLogin ObterRespostaToken(string encodedToken, IdentityUser user, IEnumerable<Claim> claims)
        {
            return new UsuarioRespostaLogin
            {
                AccessToken = encodedToken,
                ExpiresIn = TimeSpan.FromHours(_appSettings.ExpiracaoHoras).TotalSeconds,
                UsuarioToken = new UsuarioToken
                {
                    Id = user.Id,
                    Email = user.Email,
                    Claims = claims.Select(c => new UsuarioClaim { Type = c.Type, Value = c.Value })
                }
            };
        }

        // Offset padrão necessário para o JWT de datas
        private static long ToUnixEpochDate(DateTime date)
            => (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);
    }
}
