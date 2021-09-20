using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DeTodito.Models
{
    public class ProductoCarrito
    {
        [Key]
        public int Id { get; set; }
        public int IdProducto { get; set; }

        public string Nombre { get; set; }
        public int Cantidad { get; set; }
        public double  Precio { get; set; }
        public string Imagen { get; set;}
        public int PedidoId { get; set; }
    }
}
