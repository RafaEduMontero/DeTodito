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
        [Required(ErrorMessage ="El Nombre es Requerido")]
        public string Nombre { get; set; }
        [Required(ErrorMessage = "El Detalle es Requerido")]
        public string Detalle { get; set; }
        [Required(ErrorMessage = "El Precio es Requerido")]
        public double Precio { get; set; }
        public string Categoria { get; set; }
        [Required(ErrorMessage = "La imagen es Requerida")]
        public string RutaImagen { get; set; }
    }
}
