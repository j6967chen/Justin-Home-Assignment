using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TaxationService.Domain.Exceptions
{
    [Serializable]
    public abstract class TaxException : Exception
    {
        public TaxException() : base() { }

        public TaxException(string message) : base(message) { }

        public TaxException(string message, Exception inner) : base(message, inner) { }

        protected TaxException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }


    [Serializable]
    public sealed class CalculateTaxResponseException : TaxException
    {
        public CalculateTaxResponseException(string notifications) : base($"PostTaxCalculationRequestAsync returned with a failure and the following notifications: {notifications}")
        {
        }

        private CalculateTaxResponseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    [Serializable]
    public sealed class CalculateTaxRateResponseException : TaxException
    {
        public CalculateTaxRateResponseException(string notifications) : base($"GetTaxRateForLocation returned with a failure and the following notifications: {notifications}")
        {
        }

        private CalculateTaxRateResponseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
