using Microsoft.EntityFrameworkCore;

namespace ApiCubosExamenAzure.Data
{
    public class CubosContext : DbContext
    {
        public CubosContext(DbContextOptions<CubosContext> options) : base(options)
        {
        }
        public DbSet<Models.Cubo> Cubos { get; set; }
        public DbSet<Models.Usuario> Usuarios { get; set; }
        public DbSet<Models.ComprasCubo> ComprasCubos { get; set; }
    }
}
