using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages.Html;

namespace u24628299_Ass3.Models
{
    public class ReportViewModel
    {
        public List<SalesReportItem> SalesData { get; set; }
        public SecureString CharDataJson { get; set; }
        public int? SelectedCustomerId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<SelectedListItem> Customers { get; set; }

        public string ReportDescription { get; set; }
        public string FileName { get; set; }
        public List<SavedReport> SavedReports { get; set; }
        public List<StockItemReportDto> StockItems { get; set; }
        public List<OrderHistoryReportDto> OrderHistory { get; set; }

        public ReportViewModel()
        {
            SalesData = new List<SalesReportItem>();
            Customers = new List<SelectedListItem>();
            SavedReports = new List<SavedReport>();
            StockItems = new List<StockItemReportDto>();
            OrderHistory = new List<OrderHistoryReportDto>();
        }

    }
    public class SalesReportItem
    {
        public string ProductName { get; set; }
        public string CustomerName { get; set; }
        public string StaffName { get; set; }
        public DateTime SaleDate { get; set; }
        public decimal Amount { get; set; }
    }

    public class SavedReport
    {
        public string FileName { get; set; }
        public string FileType { get; set; }
        public DateTime SavedOn { get; set; }
        public DateTime DateCreated { get; set; }
        public string Description { get; set; }
    }

    public class SelectedListItem
    {
        public int Value { get; set; }
        public string Text { get; set; }
    }

    public class StockItemReportDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
    }

    public class OrderHistoryReportDto
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Amount { get; set; }
    }
}