using ApiCubosExamenAzure.Data;
using ApiCubosExamenAzure.Models;

namespace ApiCubosExamenAzure.Repositories
{
    public class RepositoryCubos
    {
        private CubosContext context;
        public RepositoryCubos(CubosContext context)
        {
            this.context = context;
        }



        public async Task<List<Cubo>> GetCubosAsync()
        {
            var consulta = from datos in this.context.Cubos
                           select datos;
            return consulta.ToList();
        }
        public async Task<List<Cubo>> GetCubosByMarcaAsync(string marca)
        {
            var consulta = from datos in this.context.Cubos
                           where datos.Marca == marca
                           select datos;
            return consulta.ToList();
        }
    }
}
