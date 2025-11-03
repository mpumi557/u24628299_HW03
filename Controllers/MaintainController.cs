using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using u24628299_Ass3.Models;

namespace u24628299_Ass3.Controllers
{
    public class MaintainController : Controller
    {
        private BikeStoresEntities db = new BikeStoresEntities();

        public async Task<ActionResult> Maintain(int? staffId, int? customerId, int? productId)
        {
            var model = new MaintainViewModel
            {
                Staffs = await db.staffs
                    .Include(s => s.stores)
                    .Include(s => s.orders.Select(o => o.order_items.Select(oi => oi.products)))
                    .ToListAsync(),
                Customers = await db.customers
                    .Include(c => c.orders.Select(o => o.order_items.Select(oi => oi.products)))
                    .ToListAsync(),
                Products = await db.products
                    .Include(p => p.brands)
                    .Include(p => p.categories)
                    .ToListAsync(),
                Stores = await db.stores.ToListAsync(),
                Brands = await db.brands.ToListAsync(),         
                Categories = await db.categories.ToListAsync()  
            };

            // Load selected staff
            if (staffId.HasValue)
            {
                model.SelectedStaff = model.Staffs.FirstOrDefault(s => s.staff_id == staffId.Value);
            }
            else
            {
                model.SelectedStaff = model.Staffs.FirstOrDefault();
            }

            // Load selected customer
            if (customerId.HasValue)
            {
                model.SelectedCustomer = model.Customers.FirstOrDefault(c => c.customer_id == customerId.Value);
            }
            else
            {
                model.SelectedCustomer = model.Customers.FirstOrDefault();
            }

            // Load selected product
            if (productId.HasValue)
            {
                model.SelectedProduct = model.Products.FirstOrDefault(p => p.product_id == productId.Value);
            }
            else
            {
                model.SelectedProduct = model.Products.FirstOrDefault();
            }

            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}