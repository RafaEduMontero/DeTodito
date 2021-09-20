using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using DeTodito.Models;

namespace DeTodito.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<DeTodito.Models.Producto> Producto { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }

        public DbSet<ProductoCarrito> ProductoCarrito { get; set; }
    }
}
