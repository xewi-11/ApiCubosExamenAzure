using ApiCubosExamenAzure.Models;
using ApiCubosExamenAzure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ApiCubosExamenAzure.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CuboController : ControllerBase
    {
        private RepositoryCubos repo;
        private string blobUrl;
        public CuboController(RepositoryCubos repo, IConfiguration configuration)
        {
            this.repo = repo;
            this.blobUrl = configuration["AzureStorage:BlobUriImages"];
        }

        [HttpGet]
        public async Task<ActionResult<List<Cubo>>> GetCubos()
        {
            List<Cubo> cubos = await this.repo.GetCubosAsync();

            foreach (Cubo cubo in cubos)
            {
                cubo.Imagen = this.blobUrl + cubo.Imagen;
            }

            return cubos;
        }

        [HttpGet("{marca}")]
        public async Task<ActionResult<List<Cubo>>> GetCubosByMarca(string marca)
        {
            List<Cubo> cubos = await this.repo.GetCubosByMarcaAsync(marca);

            foreach (Cubo cubo in cubos)
            {
                cubo.Imagen = this.blobUrl + cubo.Imagen;
            }

            return cubos;
        }
    }
}
