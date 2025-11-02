using System.Collections;
using System.Collections.Generic;
using u24628299_Ass3.Models;

namespace u24628299_Ass3.Models
{
    public class MaintainViewModel
    {
        public List<staffs> Staffs { get; set; }
        public List<customers> Customers { get; set; }
        public List<products> Products { get; set; }

        public staffs SelectedStaff { get; set; }
        public customers SelectedCustomer { get; set; }
        public products SelectedProduct { get; set; }

        public List<orders> StaffOrders { get; set; }
        public List<orders> CustomerOrders { get; set; }
        public List<order_items> ProductOrderItems { get; set; }
    }
}