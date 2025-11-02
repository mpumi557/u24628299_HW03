using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Web;
using u24628299_Ass3.Models;

namespace u24628299_Ass3.Models
{
    public class HomeViewModel
    {
        public List<staffs> Staffs { get; set; }
        public List<customers> Customers { get; set; }
        public List<products> Products { get; set; }
        public List<brands> Brands { get; set; }  // For filter dropdown
        public List<categories> Categories { get; set; }  // For filter dropdown

        // Add this property for stores dropdown
        public List<stores> Stores { get; set; }

        // Paging params
        public int StaffPage { get; set; } = 1;
        public int CustomerPage { get; set; } = 1;
        public int ProductPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;  // Adjust as needed

        // Filter params
        public int? SelectedBrandId { get; set; }
        public int? SelectedCategoryId { get; set; }

        // Total counts
        public int TotalStaffCount { get; set; }
        public int TotalCustomerCount { get; set; }
        public int TotalProductCount { get; set; }
    }
}