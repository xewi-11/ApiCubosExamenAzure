using ApiCubosExamenAzure.Data;
using ApiCubosExamenAzure.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiCubosExamenAzure.Repositories
{
    public class RepositoryUsuario
    {

        private CubosContext context;
        public RepositoryUsuario(CubosContext context)
        {
            this.context = context;


        }

        public async Task<Usuario> GetUsuarioAsync(string email, string password)
        {
            var consulta = await this.context.Usuarios.FirstOrDefaultAsync(x => x.Email == email && x.Password == password);
            return consulta;
        }

        private int GetMaxIdUsuario()
        {
            if (this.context.Usuarios.Count() == 0)
            {
                return 1;
            }
            else
            {
                return this.context.Usuarios.Max(x => x.Id) + 1;
            }
        }

        public async Task CreateUser(Usuario usuario)
        {
            usuario.Id = GetMaxIdUsuario();
            this.context.Usuarios.Add(usuario);
            await this.context.SaveChangesAsync();
        }

        public async Task<List<ComprasCubo>> GetPedidosUsuarioAsync(int idUsuario)
        {
            return await this.context.ComprasCubos
                .Where(x => x.id_usuario == idUsuario)
                .ToListAsync();
        }

        private int GetMaxIdPedido()
        {
            if (!this.context.ComprasCubos.Any())
            {
                return 1;
            }
            return this.context.ComprasCubos.Max(x => x.id_pedido) + 1;
        }

        public async Task RealizarPedidoAsync(int idUsuario, int idCubo)
        {
            ComprasCubo compra = new ComprasCubo
            {
                id_pedido = GetMaxIdPedido(),
                id_usuario = idUsuario,
                id_cubo = idCubo,
                fecha_pedido = DateTime.UtcNow
            };
            
            this.context.ComprasCubos.Add(compra);
            await this.context.SaveChangesAsync();
        }
    }
}
