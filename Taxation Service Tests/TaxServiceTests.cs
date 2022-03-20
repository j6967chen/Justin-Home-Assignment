using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TaxationService.Domain.Configurations;
using TaxationService.Domain.Exceptions;
using TaxationService.Domain.Mappers;
using TaxationService.Domain.Models;
using TaxationService.Domain.Models.TaxJarModel;
using TaxationService.Domain.Models.TaxServiceModel;
using TaxationService.Domain.ServiceCalculators;
using TaxationService.Domain.ServiceCalculators.TaxCalculationClients;

namespace Taxation_Service_Tests
{
    [TestClass]
    public class TaxServiceTests
    {
        private TaxJarConfiguration taxJarAppSettings;
        private ITaxCalculator taxJarCalculator;
        private ITaxProxyService taxProxyService;
        private TaxForOrderRequest taxForOrderRequest;

        private Mock<ITaxJarClient> taxJarClientMock;
        private Mock<IMapper> mapperMock;

        [TestInitialize]
        public void TestInitialize()
        {
            var builder = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfiguration configuration = builder.Build();

            this.taxJarAppSettings = configuration.GetSection("TaxJar").Get<TaxJarConfiguration>();


            this.taxJarClientMock = new Mock<ITaxJarClient>();
            this.mapperMock = new Mock<IMapper>();

            this.taxJarClientMock.Setup(client => client.CalculateOrderTaxAsync(It.IsAny<StringContent>(), It.IsAny<CancellationToken>()))
                            .Returns(Task.FromResult(new HttpResponseMessage()));

            this.taxForOrderRequest = new TaxForOrderRequest
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
                        }
                    }
            };

            this.taxJarCalculator = new TaxJarCalculator(this.taxJarClientMock.Object);

            this.taxProxyService = new TaxProxyService(new List<ITaxCalculator> { this.taxJarCalculator }, mapperMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(CalculateTaxForOrderRequestException))] 
        public async Task TaxProxyService_CalculateTaxForOrder_Null()
        {
            var request = new TaxForOrderRequest
            {
                CalculatorType = Calculator.TaxJar
            };

             _ = await this.taxProxyService.CalculateTaxAsync(request);

            Assert.Fail("Should have thrown CalculateTaxForOrderRequestException");
        }

        [TestMethod]
        public async Task TaxProxyService_CalculateTaxForOrder_Good()
        {
            var stringContent = new StringContent("{'tax':{'amount_to_collect':20.9,'breakdown':{'city_tax_collectable':0.0,'city_tax_rate':0.0,'city_taxable_amount':0.0,'combined_tax_rate':0.08375,'county_tax_collectable':10.92,'county_tax_rate':0.04375,'county_taxable_amount':249.5,'line_items':[{'city_amount':0.0,'city_tax_rate':0.0,'city_taxable_amount':0.0,'combined_tax_rate':0.08375,'county_amount':0.2,'county_tax_rate':0.04375,'county_taxable_amount':4.5,'id':'1','special_district_amount':0.0,'special_district_taxable_amount':0.0,'special_tax_rate':0.0,'state_amount':0.18,'state_sales_tax_rate':0.04,'state_taxable_amount':4.5,'tax_collectable':0.38,'taxable_amount':4.5},{'city_amount':0.0,'city_tax_rate':0.0,'city_taxable_amount':0.0,'combined_tax_rate':0.08375,'county_amount':10.28,'county_tax_rate':0.04375,'county_taxable_amount':235.0,'id':'2','special_district_amount':0.0,'special_district_taxable_amount':0.0,'special_tax_rate':0.0,'state_amount':9.4,'state_sales_tax_rate':0.04,'state_taxable_amount':235.0,'tax_collectable':19.68,'taxable_amount':235.0}],'shipping':{'city_amount':0.0,'city_tax_rate':0.0,'city_taxable_amount':0.0,'combined_tax_rate':0.08375,'county_amount':0.44,'county_tax_rate':0.04375,'county_taxable_amount':10.0,'special_district_amount':0.0,'special_tax_rate':0.0,'special_taxable_amount':0.0,'state_amount':0.4,'state_sales_tax_rate':0.04,'state_taxable_amount':10.0,'tax_collectable':0.84,'taxable_amount':10.0},'special_district_tax_collectable':0.0,'special_district_taxable_amount':0.0,'special_tax_rate':0.0,'state_tax_collectable':9.98,'state_tax_rate':0.04,'state_taxable_amount':249.5,'tax_collectable':20.9,'taxable_amount':249.5},'freight_taxable':true,'has_nexus':true,'jurisdictions':{'city':'MAHOPAC','country':'US','county':'PUTNAM','state':'NY'},'order_total_amount':249.5,'rate':0.08375,'shipping':10.0,'tax_source':'destination','taxable_amount':249.5}}");

            TaxJarCalculator taxJarCalculator = BuildTaxJarCalculator(stringContent);

            var mappingConfig = new MapperConfiguration(mc =>
                                                {
                                                    mc.AddProfile(new TaxJarProfile());
                                                });

            IMapper mapper = mappingConfig.CreateMapper();

            var taxProxyService = new TaxProxyService(new List<ITaxCalculator> { taxJarCalculator }, mapper);

            var result = await taxProxyService.CalculateTaxAsync(this.taxForOrderRequest, new CancellationToken());

            //Assert - assert on the mock
            Assert.IsNotNull(result);
            Assert.AreEqual(result.TotalTax, 20.9m);
        }

        [TestMethod]
        [ExpectedException(typeof(CalculateTaxResponseException))]
        public async Task TaxProxyService_CalculateTaxForOrder_Exception()
        {
            var request = new TaxForOrderRequest
            {
                CalculatorType = Calculator.None
            };

            _ = await this.taxProxyService.CalculateTaxAsync(request);

            Assert.Fail("Should have thrown CalculateTaxResponseException");
        }

        [TestMethod]
        public async Task Calculator_CalculateTaxForOrder_Null()
        {
            var request = new Tax
            {
            };

            var result = await this.taxJarCalculator.CalculateTaxAsync(request);

            Assert.IsNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(CalculateTaxResponseException))]
        public async Task Calculator_CalculateTaxForOrder_StatusNotFound()
        {
            this.taxJarClientMock.Setup(client => client.CalculateOrderTaxAsync(It.IsAny<StringContent>(), It.IsAny<CancellationToken>()))
                        .Returns(Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound)));

            var request = new Tax
            {
            };

            _ = await taxJarCalculator.CalculateTaxAsync(request);

            Assert.Fail("Should have thrown CalculateTaxResponseException");
        }

        [TestMethod]
        public async Task Calculator_CalculateTaxForOrder_GoodTaxForOrder()
        {
            var stringContent = new StringContent("{'tax':{'amount_to_collect':20.9,'breakdown':{'city_tax_collectable':0.0,'city_tax_rate':0.0,'city_taxable_amount':0.0,'combined_tax_rate':0.08375,'county_tax_collectable':10.92,'county_tax_rate':0.04375,'county_taxable_amount':249.5,'line_items':[{'city_amount':0.0,'city_tax_rate':0.0,'city_taxable_amount':0.0,'combined_tax_rate':0.08375,'county_amount':0.2,'county_tax_rate':0.04375,'county_taxable_amount':4.5,'id':'1','special_district_amount':0.0,'special_district_taxable_amount':0.0,'special_tax_rate':0.0,'state_amount':0.18,'state_sales_tax_rate':0.04,'state_taxable_amount':4.5,'tax_collectable':0.38,'taxable_amount':4.5},{'city_amount':0.0,'city_tax_rate':0.0,'city_taxable_amount':0.0,'combined_tax_rate':0.08375,'county_amount':10.28,'county_tax_rate':0.04375,'county_taxable_amount':235.0,'id':'2','special_district_amount':0.0,'special_district_taxable_amount':0.0,'special_tax_rate':0.0,'state_amount':9.4,'state_sales_tax_rate':0.04,'state_taxable_amount':235.0,'tax_collectable':19.68,'taxable_amount':235.0}],'shipping':{'city_amount':0.0,'city_tax_rate':0.0,'city_taxable_amount':0.0,'combined_tax_rate':0.08375,'county_amount':0.44,'county_tax_rate':0.04375,'county_taxable_amount':10.0,'special_district_amount':0.0,'special_tax_rate':0.0,'special_taxable_amount':0.0,'state_amount':0.4,'state_sales_tax_rate':0.04,'state_taxable_amount':10.0,'tax_collectable':0.84,'taxable_amount':10.0},'special_district_tax_collectable':0.0,'special_district_taxable_amount':0.0,'special_tax_rate':0.0,'state_tax_collectable':9.98,'state_tax_rate':0.04,'state_taxable_amount':249.5,'tax_collectable':20.9,'taxable_amount':249.5},'freight_taxable':true,'has_nexus':true,'jurisdictions':{'city':'MAHOPAC','country':'US','county':'PUTNAM','state':'NY'},'order_total_amount':249.5,'rate':0.08375,'shipping':10.0,'tax_source':'destination','taxable_amount':249.5}}");

            TaxJarCalculator taxJarCalculator = BuildTaxJarCalculator(stringContent);

            var request = new Tax
            {
            };

            //Act 
            var result = await taxJarCalculator.CalculateTaxAsync(request, new CancellationToken());

            //Assert - assert on the mock
            Assert.AreEqual(result.Tax.AmountToCollect, 20.9m);
            Assert.AreEqual(result.Tax.Breakdown.StateTaxRate, 0.04m);
        }

        private TaxJarCalculator BuildTaxJarCalculator(StringContent stringContent)
        {
            //Arrange - configure the mock
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = stringContent,
                });

            var mockFactory = new Mock<IHttpClientFactory>();

            var client = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri(this.taxJarAppSettings.ApiBaseUrl)
            };

            client.DefaultRequestHeaders.Add(HeaderNames.Authorization, $"Bearer {this.taxJarAppSettings.ApiKey}");
            client.DefaultRequestHeaders.Add("x-api-version", this.taxJarAppSettings.ApiVersion);

            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            var taxJarClient = new TaxJarClient(mockFactory.Object);

            var taxJarCalculator = new TaxJarCalculator(taxJarClient);

            return taxJarCalculator;
        }

        [TestMethod]
        public async Task Calculator_GetRateForLocation_GoodRate()
        {
            var stringContent = new StringContent("{'rate':{'city':null,'city_rate':'0.01','combined_district_rate':'0.0','combined_rate':'0.07','country':'US','country_rate':'0.0','county':'CHITTENDEN','county_rate':'0.0','freight_taxable':true,'state':'VT','state_rate':'0.06','zip':'05495-9094'}}");

            TaxJarCalculator taxJarCalculator = BuildTaxJarCalculator(stringContent);

            var rate = new Rate {
                Country = "US",
                Zip = "10016"
            };

            //Act 
            var result = await taxJarCalculator.GetRatesForLocationAsync(rate, new CancellationToken());

            //Assert - assert on the mock
            Assert.AreEqual(result.Rate.CityRate, 0.01m);
            Assert.AreEqual(result.Rate.CombinedRate, 0.07m);
        }
    }
}