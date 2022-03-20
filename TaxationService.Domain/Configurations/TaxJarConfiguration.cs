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
            get;
            set;
        }

        public string ApiVersion
        {
            get;
            
            set;
        }
        public string ApiKey 
        {
            get;

            set;
            
        }
    }
}
