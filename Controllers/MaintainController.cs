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

        public async Task<ActionResult> Maintain()
        {
            var staffs = await db.staffs.Include(s => s.stores).ToListAsync();
            var customers = await db.customers.ToListAsync();
            var products = await db.products.Include(p => p.brands).Include(p => p.categories).ToListAsync();

            var model = new MaintainViewModel
            {
                Staffs = staffs,
                Customers = customers,
                Products = products
            };
            return View(model);
        }

        public async Task<ActionResult> GetStaffDetails(int id)
        {
            var staff = await db.staffs.Include(s => s.orders).FirstOrDefaultAsync(s => s.staff_id == id);
            var orders = staff?.orders.Select(o => new {
                o.order_id,
                o.order_status,
                order_date = o.order_date.ToString("yyyy-MM-dd")
            }).ToList();
            return Json(new
            {
                staff_id = staff?.staff_id,
                first_name = staff?.first_name,
                last_name = staff?.last_name,
                orders = orders
            }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetCustomerDetails(int id)
        {
            var customer = await db.customers.FirstOrDefaultAsync(c => c.customer_id == id);
            if (customer == null)
                return Json(null, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                customer_id = customer.customer_id,
                first_name = customer.first_name,
                last_name = customer.last_name
            }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetProductDetails(int id)
        {
            var product = await db.products
                .Include(p => p.brands)
                .Include(p => p.categories)
                .FirstOrDefaultAsync(p => p.product_id == id);

            if (product == null)
                return Json(null, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                product_id = product.product_id,
                product_name = product.product_name,
                model_year = product.model_year,
                brand_name = product.brands?.brand_name,
                category_name = product.categories?.category_name
            }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetProductDetailsAndOrders(int id)
        {
            var product = await db.products
                .Include(p => p.brands)
                .Include(p => p.categories)
                .FirstOrDefaultAsync(p => p.product_id == id);

            if (product == null)
                return Json(null, JsonRequestBehavior.AllowGet);

            // Get all completed order items for this product
            var soldItems = await (from oi in db.order_items
                                   join o in db.orders on oi.order_id equals o.order_id
                                   where oi.product_id == id && o.order_status == 4
                                   orderby o.order_date
                                   select new
                                   {
                                       o.order_id,
                                       o.order_date,
                                       o.customer_id,
                                       customer_name = o.customers.first_name + " " + o.customers.last_name,
                                       oi.quantity
                                   }).ToListAsync();

            return Json(new
            {
                product_id = product.product_id,
                product_name = product.product_name,
                model_year = product.model_year,
                brand_name = product.brands?.brand_name,
                category_name = product.categories?.category_name,
                soldItems = soldItems
            }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetStaffSoldItems(int id)
        {
            var soldItems = await (from o in db.orders
                                   join oi in db.order_items on o.order_id equals oi.order_id
                                   join p in db.products on oi.product_id equals p.product_id
                                   where o.staff_id == id && o.order_status == 4
                                   orderby o.order_date
                                   select new
                                   {
                                       o.order_id,
                                       o.order_date,
                                       p.product_id,
                                       p.product_name,
                                       oi.quantity,
                                       oi.list_price,
                                       oi.discount
                                   }).ToListAsync();

            return Json(soldItems, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetCustomerPurchasedItems(int id)
        {
            var purchasedItems = await (from o in db.orders
                                        join oi in db.order_items on o.order_id equals oi.order_id
                                        join p in db.products on oi.product_id equals p.product_id
                                        where o.customer_id == id && o.order_status == 4
                                        orderby o.order_date
                                        select new
                                        {
                                            o.order_id,
                                            o.order_date,
                                            o.order_status,
                                            p.product_id,
                                            p.product_name,
                                            oi.quantity,
                                            oi.list_price,
                                            oi.discount,
                                            total_price = oi.quantity * oi.list_price * (1 - oi.discount)
                                        }).ToListAsync();

            return Json(purchasedItems, JsonRequestBehavior.AllowGet);
        }
    }
}