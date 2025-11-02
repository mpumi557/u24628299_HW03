using System.Collections.Generic;
using System.Linq;
using u24628299_Ass3.Models;

namespace u24628299_Ass3.Repository
{
    public class ReportRepository
    {
        private BikeStoresEntities db = new BikeStoresEntities();

        // Stock Items Report Products in stock not sold
        public List<StockItemReportDto> GetUnsoldStockItems()
        {
            var unsold = (from s in db.stocks
                          join p in db.products on s.product_id equals p.product_id
                          where !db.order_items.Any(oi => oi.product_id == p.product_id)
                          select new StockItemReportDto
                          {
                              ProductId = p.product_id,
                              ProductName = p.product_name,
                              Quantity = s.quantity ?? 0
                          }).ToList();

            return unsold;
        }
    }
}