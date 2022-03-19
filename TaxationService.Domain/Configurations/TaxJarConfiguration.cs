using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxationService.Domain.Configurations
{
    public sealed class TaxJarConfiguration
    {
        public string ApiBaseUrl 
        { 
            get
            { 
                return "https://api.taxjar.com"; 
            }
        }

        public string ApiVersion
        {
            get
            {
                return "2022-01-24";
            }
        }
        public string ApiKey 
        {
            get
            {
                return "5da2f821eee4035db4771edab942a4cc";
            }
        }
    }
}
