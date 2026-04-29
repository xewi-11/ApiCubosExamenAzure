using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiCubosExamenAzure.Models
{

    [Table("CUBOS")]
    public class Cubo
    {
        [Key]
        [Column("id_cubo")]
        public int Id { get; set; }
        [Column("nombre")]
        public string Nombre { get; set; }
        [Column("marca")]
        public string Marca { get; set; }
        [Column("precio")]
        public int Precio { get; set; }
        [Column("imagen")]
        public string Imagen { get; set; }
    }
}
