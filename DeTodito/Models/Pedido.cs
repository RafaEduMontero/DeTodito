using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DeTodito.Models
{
    public class Pedido
    {
        [Key]
        public int Id { get; set; }
        public string Domicilio { get; set; }
        public string DetalleEnvio { get; set; }
        public string EstadoEnvio { get; set; }
        public ICollection<ProductoCarrito> Productos { get; set; }
        public string IdCliente { get; set; }
    }
}
