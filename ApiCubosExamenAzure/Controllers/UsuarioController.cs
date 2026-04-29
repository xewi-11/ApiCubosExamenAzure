using ApiCubosExamenAzure.Models;
using ApiCubosExamenAzure.Repositories;
using ApiCubosExamenAzure.Services;
using ApiOAuthEmpleados.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace ApiCubosExamenAzure.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private RepositoryUsuario repo;
        private BlobService serviceStorage;

        public UsuarioController(RepositoryUsuario repo, BlobService serviceStorage)
        {
            this.repo = repo;
            this.serviceStorage = serviceStorage;
        }

        [HttpPost]
        [Route("CreateUser")]
        public async Task<IActionResult> CreateUser([FromForm] Usuario usuario, IFormFile file)
        {
            if (file != null)
            {
                // Solo guardamos el nombre de la foto en la base de datos
                usuario.imagen = file.FileName;

                using var stream = file.OpenReadStream();
                // Pasamos el contenedor a utilizar y el nombre de la foto
                await this.serviceStorage.UploadBlobAsync("suarioscubos", file.FileName, stream);
            }

            await this.repo.CreateUser(usuario);
            return Ok(new { message = "Usuario creado correctamente" });
        }

        private Usuario? ObtenerUsuarioDelToken()
        {
            Claim? userDataClaim = User.Claims.FirstOrDefault(c => c.Type == "UserData");
            if (userDataClaim is null) return null;

            string userDataCifrada = userDataClaim.Value;
            string userDataJson = HelperCryptography.DescifrarString(userDataCifrada);
            return JsonSerializer.Deserialize<Usuario>(userDataJson);
        }

        [Authorize]
        [HttpGet]
        [Route("[action]")]
        public async Task<ActionResult> Perfil()
        {
            try
            {
                Usuario? userData = ObtenerUsuarioDelToken();
                if (userData is null)
                {
                    return BadRequest("Token no contiene UserData o no se pudo deserializar.");
                }

                // Si el usuario tiene imagen, crear la URL completa usando Shared Access Signatures (SAS)
                if (!string.IsNullOrEmpty(userData.imagen))
                {
                    string? sasUrl = this.serviceStorage.GenerateSasToken("suarioscubos", userData.imagen);
                    if (sasUrl != null)
                    {
                        userData.imagen = sasUrl;
                    }
                    else 
                    {
                        // Fallback por si la generación de SAS falla por configuraciones
                        string blobContainerUrl = "https://storagetajamarjam.blob.core.windows.net/suarioscubos/";
                        userData.imagen = blobContainerUrl + userData.imagen;
                    }
                }
                
                return Ok(userData);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al procesar el token: {ex.Message}");
            }
        }

        [Authorize]
        [HttpGet]
        [Route("[action]")]
        public async Task<ActionResult> Pedidos()
        {
            try
            {
                Usuario? userData = ObtenerUsuarioDelToken();
                if (userData is null)
                {
                    return BadRequest("Token no contiene UserData o no se pudo deserializar.");
                }

                var pedidos = await this.repo.GetPedidosUsuarioAsync(userData.Id);
                return Ok(pedidos);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al procesar el token: {ex.Message}");
            }
        }

        [Authorize]
        [HttpPost]
        [Route("[action]/{idCubo}")]
        public async Task<ActionResult> RealizarPedido(int idCubo)
        {
            try
            {
                Usuario? userData = ObtenerUsuarioDelToken();
                if (userData is null)
                {
                    return BadRequest("Token no contiene UserData o no se pudo deserializar.");
                }

                await this.repo.RealizarPedidoAsync(userData.Id, idCubo);
                return Ok(new { message = "Pedido realizado correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al procesar el token: {ex.Message}");
            }
        }
    }
}
