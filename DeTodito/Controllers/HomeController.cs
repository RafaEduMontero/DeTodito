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
using MercadoPago.Client.Payment;
using MercadoPago.Config;
using MercadoPago.Resource.Payment;
using MercadoPago.Resource.Preference;
using MercadoPago.Client.Preference;



namespace DeTodito.Controllers
{
    public class HomeController : Controller
    {

        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? idproducto, string? cantidad)
        {

            var productos = await _context.Producto.ToListAsync();
            if (idproducto != null)
            {
                List<int> carrito;
                List<ProductoCarrito> lista;
                if (HttpContext.Session.GetObject<List<int>>("CARRITO") == null)
                {
                    carrito = new List<int>();
                    lista = new List<ProductoCarrito>();
                }
                else
                {
                    carrito = HttpContext.Session.GetObject<List<int>>("CARRITO");
                    lista = HttpContext.Session.GetObject<List<ProductoCarrito>>("CARRITO1");
                }
                if (carrito.Contains(idproducto.Value) == false)
                {
                    carrito.Add(idproducto.Value);
                    HttpContext.Session.SetObject("CARRITO", carrito);
                    ProductoCarrito productoCarrito = new ProductoCarrito();
                    Producto p = productos.Where(p => p.IdProducto == idproducto).FirstOrDefault<Producto>();
                    productoCarrito.IdProducto = p.IdProducto;
                    productoCarrito.Nombre = p.Nombre;
                    productoCarrito.Precio = p.Precio;
                    productoCarrito.Imagen = p.RutaImagen;
                    productoCarrito.Cantidad = int.Parse(cantidad);
                    lista.Add(productoCarrito);
                    HttpContext.Session.SetObject("CARRITO1", lista);
                }
            }

            return View(productos);

        }

        public IActionResult Carrito(int? idproducto)
        {
            List<ProductoCarrito> carrito = HttpContext.Session.GetObject<List<ProductoCarrito>>("CARRITO1");
            List<int> carrito1 = HttpContext.Session.GetObject<List<int>>("CARRITO");
            if (carrito == null)
            {
                return View();
            }
            else
            {
                if (idproducto != null)
                {
                    carrito1.Remove(idproducto.Value);
                    ProductoCarrito p = carrito.Where(ec => ec.IdProducto == idproducto).FirstOrDefault<ProductoCarrito>();
                    carrito.Remove(p);
                    HttpContext.Session.SetObject("CARRITO1", carrito);
                    HttpContext.Session.SetObject("CARRITO", carrito1);
                }

                return View(carrito);
            }
        }

        [HttpPost]
        public IActionResult Carrito(ProductoCarrito p, string Nombre)
        {
            List<ProductoCarrito> carrito = HttpContext.Session.GetObject<List<ProductoCarrito>>("CARRITO1");
            Console.WriteLine(Nombre);
            ProductoCarrito produ = carrito.Where(p => p.Nombre == Nombre).FirstOrDefault<ProductoCarrito>();

            var indice = carrito.IndexOf(produ);
            carrito.RemoveAt(indice);
            carrito.Insert(indice, p);
            HttpContext.Session.SetObject("CARRITO1", carrito);
            return View(carrito);
        }

        public async Task<IActionResult> Pedidos()
        {
            List<ProductoCarrito> carrito = HttpContext.Session.GetObject<List<ProductoCarrito>>("CARRITO1");

            MercadoPagoConfig.AccessToken = "Tu token MP";
            List<PreferenceItemRequest> Items1 = new List<PreferenceItemRequest>();
            foreach (var producto in carrito)
            {
                var item = new PreferenceItemRequest
                {
                    Title = producto.Nombre,
                    Quantity = producto.Cantidad,
                    CurrencyId = "ARS",
                    UnitPrice = decimal.Parse(producto.Precio.ToString())

                };
                Items1.Add(item);
            }

            var request = new PreferenceRequest
            {
                Items = Items1,
                BackUrls = new PreferenceBackUrlsRequest
                {
                    Success = "https://localhost:44338/Home/Retorno",
                    Failure = "http://www.tu-sitio/failure",
                    Pending = "http://www.tu-sitio/pendings",
                },
                AutoReturn = "approved",
            };

            // Crea la preferencia usando el client
            var client = new PreferenceClient();
            var preference = await client.CreateAsync(request);
            ViewBag.Preferencia = preference.InitPoint;


            HttpContext.Session.Remove("CARRITO");
            HttpContext.Session.Remove("CARRITO1");
            return View(carrito);
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
        
        public IActionResult Retorno()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Retorno(string? url)
        {
            string ejemplo = HttpContext.Request.Path;
            Console.WriteLine(ejemplo);
            ViewBag.Ejemplo = ejemplo;
            return View(url);
        }
    }
}


