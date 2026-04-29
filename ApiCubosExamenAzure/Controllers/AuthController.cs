
using ApiCubosExamenAzure.Models;
using ApiCubosExamenAzure.Repositories;
using ApiOAuthEmpleados.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace AyudaExamenAzure.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private RepositoryUsuario repo;
        private HelperActionOAuthService helper;

        public AuthController(RepositoryUsuario repo, HelperActionOAuthService helper)
        {
            this.repo = repo;
            this.helper = helper;
        }
        [Authorize]
        [HttpGet]
        [Route("[action]")]

        public async Task<ActionResult> Perfil()
        {
            try
            {
                Claim? userDataClaim = User.Claims.FirstOrDefault(c => c.Type == "UserData");
                if (userDataClaim is null)
                {
                    return BadRequest("Token no contiene UserData.");
                }

                string userDataCifrada = userDataClaim.Value;
                string userDataJson = HelperCryptography.DescifrarString(userDataCifrada);
                var userData = JsonSerializer.Deserialize<Usuario>(userDataJson);
                if (userData is null)
                {
                    return BadRequest("No se pudo deserializar UserData.");
                }
                
                // Devolvemos el modelo de usuario completo
                return Ok(userData);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al procesar el token: {ex.Message}");
            }
        }


        [HttpPost]
        [Route("[action]")]
        public async Task<ActionResult> Login(LoginModel model)
        {
            // Cambiado a model.email ya que es lo que envías al método y se asume por el modelo
            if (string.IsNullOrWhiteSpace(model.email) || string.IsNullOrWhiteSpace(model.Password))
            {
                return BadRequest("Email y password son obligatorios.");
            }

            Usuario? user = await this.repo.GetUsuarioAsync(model.email, model.Password);
            if (user is null)
            {
                return Unauthorized();
            }

            string token = this.CreateToken(user);
            return Ok(new { response = token });
        }
        
        private string CreateToken(Usuario user)
        {
            SigningCredentials credentials = new(this.helper.GetKeyToken(), SecurityAlgorithms.HmacSha256);

            // Serializamos el usuario entero para que al leerlo en Perfil tengamos todo el modelo Usuario
            string userJson = JsonSerializer.Serialize(user);
            string userDataCifrada = HelperCryptography.CifrarString(userJson);

            List<Claim> claims = new()
            {
                new Claim("UserData", userDataCifrada)
            };

            JwtSecurityToken token = new(
                claims: claims,
                issuer: this.helper.Issuer,
                audience: this.helper.Audience,
                signingCredentials: credentials,
                expires: DateTime.UtcNow.AddMinutes(60),
                notBefore: DateTime.UtcNow
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}