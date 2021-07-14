using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DeTodito.Models
{
    public class Producto
    {
        [Key]
        public int IdProducto { get; set; }
        public string Nombre { get; set; }
        public string Detalle { get; set; }
        public double Precio { get; set; }
        public string Categoria { get; set; }
        public string RutaImagen { get; set; }
    }
}
