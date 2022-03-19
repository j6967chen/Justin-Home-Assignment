using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxationService.Domain.Models.TaxServiceModel
{
    public class RateForLocation
    {
        public string Country { get; set; }
        public decimal CombindRate { get; set; }            
    }
}
