using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxationService.Domain.Models.TaxServiceModel
{
    public class TaxForOrderResponse
    {
        public decimal OrderTotalAmount { get; set; }
        public decimal TotalTax { get; set; }
        public decimal TaxableShipping { get; set; }
    }
}
