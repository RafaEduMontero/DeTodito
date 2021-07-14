using DeTodito.Data;
using DeTodito.Extensions;
using DeTodito.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;


namespace DeTodito.Controllers
{
    public class HomeController : Controller
    {

        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? idproducto)
        {
            if(idproducto != null)
            {
                List<int> carrito;
                if (HttpContext.Session.GetObject<List<int>>("CARRITO") == null)
                {
                    carrito = new List<int>();
                }
                else
                {
                    carrito = HttpContext.Session.GetObject<List<int>>("CARRITO");
                }
                if(carrito.Contains(idproducto.Value) == false)
                {
                    carrito.Add(idproducto.Value);
                    HttpContext.Session.SetObject("CARRITO", carrito);
                }
            }
            var productos = await _context.Producto.ToListAsync();
            return View(productos);

        }

        public async Task<IActionResult> Carrito(int? idproducto)
        {
            List<int> carrito = HttpContext.Session.GetObject<List<int>>("CARRITO");
            if(carrito == null)
            {
                return View();
            }
            else
            {
                if(idproducto != null)
                {
                    carrito.Remove(idproducto.Value);
                    HttpContext.Session.SetObject("CARRITO", carrito);
                }

                var productos = await _context.Producto.ToListAsync();
                var productosCarrito = productos.Where(p => carrito.Contains(p.IdProducto));
                return View(productosCarrito);

                //return View(await _context.Producto.ToListAsync());
            }
        }

        public async Task<IActionResult> Pedidos()
        {
            List<int> carrito = HttpContext.Session.GetObject<List<int>>("CARRITO");
            var productos = await _context.Producto.ToListAsync();
            var productosCarrito = productos.Where(p => carrito.Contains(p.IdProducto));

            HttpContext.Session.Remove("CARRITO");
            return View(productosCarrito);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}


