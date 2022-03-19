using Microsoft.Extensions.DependencyInjection;
using TaxationService.Domain.Configurations;
using TaxationService.Domain.Exceptions;
using TaxationService.Domain.Models.TaxServiceModel;
using TaxationService.Domain.ServiceCalculators;
using TaxationService.Domain.ServiceCalculators.TaxCalculationClients;

namespace MyApp
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            //Dependency Injection for service builder.
            var serviceProvider = new ServiceCollection()
                                        .AddTransient<ITaxationProxyService, TaxationProxyService>()
                                        .AddTransient<ITaxCalculator, TaxJarCalculator>()
                                        .AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies())
                                        .AddSingleton(new TaxJarConfiguration())
                                        .AddTransient<ITaxJarClient, TaxJarClient>()
                                        .AddHttpClient()
                                        .BuildServiceProvider();

            //standup the Tax proxy service. 
            var taxationProxyService = serviceProvider.GetService<ITaxationProxyService>();

            if (taxationProxyService != null)
            {
                await RunTaxForOrder(taxationProxyService);

                await RunRateForLocation(taxationProxyService);
            }
        }

        private static async Task RunTaxForOrder(ITaxationProxyService taxationProxyService)
        {
            var request = new TaxForOrderRequest
            {
                Amount = 10,
                CalculatorType = Calculator.TaxJar,
                Seller = new CalculateTaxRequestSeller
                {
                    Street = "350 5th ave",
                    City = "New York",
                    State = "NY",
                    Zip = "10118",
                    Country = "US"
                },
                CustomerAddress = new CustomerAddress
                {
                    Street = "680 Route Six",
                    City = "Mahopac",
                    State = "NY",
                    Zip = "10541",
                    Country = "US"
                },
                Shipping = 10,
                LineItems = new List<LineItemRequest>
                    {
                        new LineItemRequest
                        {
                            Id = "1",
                            Quantity = 1,
                            UnitPrice = 4.50m,
                        },
                        new LineItemRequest
                        {
                            Id = "2",
                            Quantity = 10,
                            UnitPrice = 23.50m,
                        }
                    }
            };

            var calculatorOrderTax = await taxationProxyService.CalculateTaxAsync(request);

            Console.WriteLine($"CalculateOrderTAx: {calculatorOrderTax.TotalTax}");
        }

        private async static Task RunRateForLocation(ITaxationProxyService taxationProxyService)
        {
            try
            {
                var taxRateRequest = new TaxRateRequest
                {
                    CalculatorType = Calculator.TaxJar,
                    Country = "US",
                    Zip = "05495-9094",
                    City = "great neck"
                };

                var rateForLocation = await taxationProxyService.GetRatesForLocationAsync(taxRateRequest);

                Console.WriteLine($"RateForLocation: {rateForLocation.CombindRate}, Country: {rateForLocation.Country}");
            }
            catch (CalculateTaxRateResponseException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
