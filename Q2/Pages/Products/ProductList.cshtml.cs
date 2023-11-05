using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Q2.Models;
using System.Text.Json;

namespace Q2.Pages.Products
{
    public class ProductListModel : PageModel
    {
        private readonly LuyenOnThiDBContext context;
        private readonly IHubContext<ProductHub> hubContext;
        public ProductListModel(IHubContext<ProductHub> hubContext)
        {
            context = new LuyenOnThiDBContext();
            this.hubContext = hubContext;
        }
        public List<Product> products { get; set; }
        public List<Category> categories { get; set; }
        public int CategorySelected { get; set; }
        public void OnGet(int idCate = 0)
        {
            if (idCate == 0)
            {
                products = context.Products.Include(p => p.Category).ToList();
            }
            else
            {
                CategorySelected = idCate;
                products = context.Products.Include(p => p.Category).Where(p => p.CategoryId == idCate).ToList();
            }
            categories = context.Categories.ToList();
        }
        public IActionResult OnGetAddToCart(int productId, int idCate)
        {
            List<OrderDetail>? orders = new List<OrderDetail>();
            if(HttpContext.Session.GetString("cart") != null)
            {
                string? data = HttpContext.Session.GetString("cart");
                orders = JsonSerializer.Deserialize<List<OrderDetail>>(data);  
            }
            else
            {
                orders = new List<OrderDetail>();
            }

            OrderDetail? order = orders.FirstOrDefault(x => x.ProductId == productId);
            if (order != null)
            {
                order.Quantity++;
            }else
            {
                order = new OrderDetail();
                order.Quantity = 1;
                order.ProductId = productId;
                orders.Add(order);
            }

            HttpContext.Session.SetString("cart", JsonSerializer.Serialize(orders));
            CategorySelected = idCate;
            return RedirectToPage("", new {idCate = idCate});
        }

        public IActionResult OnGetDeleteProduct(int productId, int idCate)
        {
            Product product = context.Products.Include(p => p.OrderDetails).FirstOrDefault(x => x.ProductId == productId);
            if(product != null)
            {
                List<OrderDetail>? orders = new List<OrderDetail>();
                if (HttpContext.Session.GetString("cart") != null)
                {
                    string? data = HttpContext.Session.GetString("cart");
                    orders = JsonSerializer.Deserialize<List<OrderDetail>>(data);
                }
                else
                {
                    orders = new List<OrderDetail>();
                }

                OrderDetail? order = orders.FirstOrDefault(x => x.ProductId == productId);
                if (order != null)
                {
                    orders.Remove(order);
                    HttpContext.Session.SetString("cart", JsonSerializer.Serialize(orders));
                }

                context.OrderDetails.RemoveRange(product.OrderDetails);
                context.Products.Remove(product);
                context.SaveChanges();
                hubContext.Clients.All.SendAsync("ProductDeleted", productId);
            }
            
            CategorySelected = idCate;
            return RedirectToPage("", new { idCate = idCate });
        }
    }
}
