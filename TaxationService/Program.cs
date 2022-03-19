using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
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
            var host = new HostBuilder()
                        .ConfigureServices(ConfigureServices).Build();

            using var serviceScope = host.Services.CreateScope();

            var services = serviceScope.ServiceProvider;

            var taxationProxyService = services.GetRequiredService<ITaxationProxyService>();

            if (taxationProxyService != null)
            {
                await RunTaxForOrder(taxationProxyService);

                await RunRateForLocation(taxationProxyService);
            }

            Console.WriteLine("Success");
        }

        private static async Task RunTaxForOrder(ITaxationProxyService taxationProxyService)
        {
            try
            {
                var request = new TaxForOrderRequest
                {
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
                    Amount = 10,
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
            catch (CalculateTaxResponseException ex)
            { 
                Console.WriteLine(ex.Message);
            }
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

        private static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
        {

            var builder = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfiguration configuration = builder.Build();

            var taxJarAppSettings = configuration.GetSection("TaxJar").Get<TaxJarConfiguration>();

            services.AddTransient<ITaxationProxyService, TaxationProxyService>();

            services.AddTransient<ITaxCalculator, TaxJarCalculator>();

            services.AddTransient<ITaxJarClient, TaxJarClient>();

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            //set up httpClient with base url, api-version and auth api key.
            services.AddHttpClient("TaxJar", client => {
                client.BaseAddress = new Uri(taxJarAppSettings.ApiBaseUrl);
                client.DefaultRequestHeaders.Add(HeaderNames.Authorization, $"Bearer {taxJarAppSettings.ApiKey}");
                client.DefaultRequestHeaders.Add("x-api-version", taxJarAppSettings.ApiVersion);
            });
        }
    }
}
