using Microsoft.ApplicationInsights.Channel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MimeoTaxationService.Tests
{
    public class FakeTelemetryChannel : ITelemetryChannel
    {
        public ConcurrentBag<ITelemetry> SentTelemtries = new ConcurrentBag<ITelemetry>();

        public bool IsFlushed { get; private set; }

        public bool? DeveloperMode { get; set; }

        public string EndpointAddress { get; set; }

        public void Send(ITelemetry item)
        {
            SentTelemtries.Add(item);
        }

        public void Flush()
        {
            IsFlushed = true;
        }

        public void Dispose()
        {

        }
    }
}
