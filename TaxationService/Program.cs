using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using TaxationService.Domain.Configurations;
using TaxationService.Domain.Exceptions;
using TaxationService.Domain.Models.TaxServiceModel;
using TaxationService.Domain.ServiceCalculators;
using TaxationService.Domain.ServiceCalculators.TaxCalculationClients;

namespace MyApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var serviceProvider = new ServiceCollection()
                                    .AddLogging()
                                    .AddTransient<ITaxationProxyService, TaxationProxyService>()
                                    .AddTransient<ITaxCalculator, TaxJarCalculator>()
                                    .AddTransient<ITaxJarClient, TaxJarClient>()
                                    .AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies())
                                    .AddSingleton(new TaxJarConfiguration())
                                    .AddHttpClient()
                                    .BuildServiceProvider();

            var taxationProxyService = serviceProvider.GetService<ITaxationProxyService>();

            if (taxationProxyService != null)
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

                var calculatorOrderTax = taxationProxyService.CalculateTaxAsync(request).GetAwaiter().GetResult();

                Console.WriteLine($"CalculateOrderTAx: {calculatorOrderTax.TotalTax}");

                try
                {
                    var taxRateRequest = new TaxRateRequest
                    {
                        CalculatorType = Calculator.TaxJar,
                        Country = "UssS",
                        Zip = "11021",
                        City = "great neck"
                    };

                    var rateForLocation = taxationProxyService.GetRatesForLocationAsync(taxRateRequest).GetAwaiter().GetResult();

                    Console.WriteLine($"RateForLocation: {rateForLocation.CombindRate}");
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
}
