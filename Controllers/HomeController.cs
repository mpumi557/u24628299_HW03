using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using u24628299_Ass3.Models;

namespace u24628299_Ass3.Controllers
{
    public class HomeController : Controller
    {
        private BikeStoresEntities db = new BikeStoresEntities();

        public async Task<ActionResult> Index(
            int staffPage = 1,
            int customerPage = 1,
            int productPage = 1,
            int? brandId = null,
            int? categoryId = null)
        {
            int pageSize = 1; // <-- Change here

            // Staffs (async with join to Store)
            var staffsQuery = db.staffs.Include(s => s.stores);
            var staffs = await staffsQuery
                .OrderBy(s => s.staff_id)
                .Skip((staffPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            int totalStaffCount = await staffsQuery.CountAsync();

            // Customers (async)
            var customersQuery = db.customers;
            var customers = await customersQuery
                .OrderBy(c => c.customer_id)
                .Skip((customerPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            int totalCustomerCount = await customersQuery.CountAsync();

            // Brands and Categories (async)
            var brands = await db.brands.ToListAsync();
            var categories = await db.categories.ToListAsync();

            // Products with paging, filtering, and joins (async)
            var productsQuery = db.products.Include(p => p.brands).Include(p => p.categories);
            if (brandId.HasValue)
                productsQuery = productsQuery.Where(p => p.brand_id == brandId.Value);
            if (categoryId.HasValue)
                productsQuery = productsQuery.Where(p => p.category_id == categoryId.Value);

            int totalProductCount = await productsQuery.CountAsync();
            var products = await productsQuery
                .OrderBy(p => p.product_id)
                .Skip((productPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var stores = await db.stores.ToListAsync(); // If needed for joins/display

            var model = new HomeViewModel
            {
                Staffs = staffs,
                TotalStaffCount = totalStaffCount,
                StaffPage = staffPage,
                Customers = customers,
                TotalCustomerCount = totalCustomerCount,
                CustomerPage = customerPage,
                Brands = brands,
                Categories = categories,
                Products = products,
                SelectedBrandId = brandId,
                SelectedCategoryId = categoryId,
                Stores = stores,
                PageSize = pageSize,
                TotalProductCount = totalProductCount,
                ProductPage = productPage
            };

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