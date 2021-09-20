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
using System.Net;
using System.IO;
using Microsoft.AspNetCore.Identity;

namespace DeTodito.Controllers
{
    public class HomeController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        public HomeController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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

            MercadoPagoConfig.AccessToken = "APP_USR-6623451607855904-111502-1f258ab308efb0fb26345a2912a3cfa5-672708410";
            List<PreferenceItemRequest> Items1 = new List<PreferenceItemRequest>();
            foreach (var p in carrito)
            {
                var item = new PreferenceItemRequest
                {
                    Title = p.Nombre,
                    Quantity = p.Cantidad,
                    CurrencyId = "ARS",
                    UnitPrice = decimal.Parse(p.Precio.ToString())
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
            return View(carrito);
        }

        [HttpPost]
        public IActionResult Pedidos(string preferencia,string domicilio,string detalleEnvio)
        {
            var envio = new Envio
            {
                Domicilio = domicilio,
                DetalleDomicilio = detalleEnvio
            };
            HttpContext.Session.SetObject("DOMICILIO", envio);
            return Redirect(preferencia);
        }

        public async Task<IActionResult> DespachoPedidos()
        {
            return View(await _context.Pedidos.Include(p => p.Productos).ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> DespachoPedidos(int idpedido, string estado)
        {
            if(estado != "nada")
            {
                var originalPedido = await _context.Pedidos.FindAsync(idpedido);
                originalPedido.EstadoEnvio = estado;
                _context.Update(originalPedido);
                await _context.SaveChangesAsync();
            }

            return View(await _context.Pedidos.Include(p => p.Productos).ToListAsync());
        }

        public async Task<IActionResult> PedidosCliente()
        {
            var user = _userManager.FindByNameAsync(User.Identity.Name).Result;
            var pedidos = await _context.Pedidos.Include(p => p.Productos).ToListAsync();
            var pedidosCliente = pedidos.Where(p => p.IdCliente == user.Id).ToList();
            return View(pedidosCliente);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }    
        
        [HttpGet]
        public IActionResult Retorno()
        {
            var user = _userManager.FindByNameAsync(User.Identity.Name).Result;
            List<ProductoCarrito> carrito = HttpContext.Session.GetObject<List<ProductoCarrito>>("CARRITO1");
            var envio = HttpContext.Session.GetObject<Envio>("DOMICILIO");
            var pedido = new Pedido
            {
                Domicilio = envio.Domicilio,
                DetalleEnvio = envio.DetalleDomicilio,
                EstadoEnvio = "No Enviado",
                Productos = carrito,
                IdCliente = user.Id
            };

            _context.Add(pedido);
            _context.SaveChanges();
            HttpContext.Session.Remove("CARRITO");
            HttpContext.Session.Remove("CARRITO1");
            HttpContext.Session.Remove("DOMICILIO");
            return View();
        }

        //private static void GetItem(int id)
        //{
        //    var url = $"http://localhost:8080/item/{id}";
        //    var request = (HttpWebRequest)WebRequest.Create(url);
        //    request.Method = "GET";
        //    request.ContentType = "application/json";
        //    request.Accept = "application/json";
        //    try
        //    {
        //        using (WebResponse response = request.GetResponse())
        //        {
        //            using (Stream strReader = response.GetResponseStream())
        //            {
        //                if (strReader == null) return;
        //                using (StreamReader objReader = new StreamReader(strReader))
        //                {
        //                    string responseBody = objReader.ReadToEnd();
        //                    // Do something with responseBody
        //                    Console.WriteLine(responseBody);
        //                }
        //            }
        //        }
        //    }
        //    catch (WebException ex)
        //    {
        //        // Handle error
        //    }
        //}
        ////[HttpPost]
        ////public IActionResult Retorno(PaymentOrderRequest p)
        ////{
        ////    string ejemplo = HttpContext.Request.Path;
        ////    Console.WriteLine(ejemplo);
        ////    ViewBag.Ejemplo = ejemplo;
        ////    return View();
        ////}
    }
}


