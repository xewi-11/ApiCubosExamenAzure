using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiCubosExamenAzure.Models
{
    [Table("COMPRACUBOS")]
    public class ComprasCubo
    {
        [Key]
        [Column("id_pedido")]
        public int id_pedido { get; set; }
        [Column("id_usuario")]
        public int id_usuario { get; set; }
        [Column("id_cubo")]
        public int id_cubo { get; set; }
        [Column("fechapedido")]
        public DateTime fecha_pedido { get; set; }

    }
}
