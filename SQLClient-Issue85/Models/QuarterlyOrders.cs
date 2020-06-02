using System;
using System.Collections.Generic;

namespace SQLClient_Issue85.Models
{
    public partial class QuarterlyOrders
    {
        public string CustomerId { get; set; }
        public string CompanyName { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
    }
}
