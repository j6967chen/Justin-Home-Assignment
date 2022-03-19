using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxationService.Domain.Models.TaxServiceModel
{
    public class TaxRateRequest
    {
        public Calculator CalculatorType { get; set; }

        public string Country { get; set; }
        public string Zip { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
    }
}
