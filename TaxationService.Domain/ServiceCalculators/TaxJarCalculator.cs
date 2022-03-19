using Newtonsoft.Json;
using System.Text;
using TaxationService.Domain.Exceptions;
using TaxationService.Domain.Models;
using TaxationService.Domain.Models.TaxJarModel;
using TaxationService.Domain.Models.TaxServiceModel;
using TaxationService.Domain.ServiceCalculators.TaxCalculationClients;

namespace TaxationService.Domain.ServiceCalculators
{
    public interface ITaxCalculator
    {
        Calculator GetCalculatorType { get; }

        Task<TaxResponse> CalculateTaxAsync(Tax request, CancellationToken cancellationToken = default);

        Task<TaxRateResponse> GetRatesForLocationAsync(Rate request, CancellationToken cancellationToken = default);
    }

    public class TaxJarCalculator : ITaxCalculator
    {
        private readonly ITaxJarClient taxJarClient;

        private const Calculator calculatorType = Calculator.TaxJar;

        public TaxJarCalculator(ITaxJarClient taxJarClient)
        { 
            this.taxJarClient = taxJarClient;
        }

        public Calculator GetCalculatorType => calculatorType;

        public async Task<TaxRateResponse> GetRatesForLocationAsync(Rate request, CancellationToken cancellationToken = default)
        {
            var response = await this.taxJarClient.GetRatesAsync(request, cancellationToken).ConfigureAwait(false);

            try
            {
                response.EnsureSuccessStatusCode();

                var responseData = await response.Content.ReadAsStringAsync(cancellationToken);

                var getTaxRateResponse = JsonConvert.DeserializeObject<TaxRateResponse>(responseData);

                if (getTaxRateResponse != null)
                {
                    return getTaxRateResponse;
                }

                return await Task.FromResult(default(TaxRateResponse));
            }
            catch (Exception exception)
            {
                throw new CalculateTaxRateResponseException(exception.Message);
            }
        }

        public async Task<TaxResponse> CalculateTaxAsync(Tax request, CancellationToken cancellationToken)
        {
            var stringContent = TaxJarCalculator.ConvertRequestToJsonHttpContent(request);

            var response = await this.taxJarClient.CalculateOrderTaxAsync(stringContent, cancellationToken);

            try
            {
                response.EnsureSuccessStatusCode();

                var responseData = await response.Content.ReadAsStringAsync(cancellationToken);

                var calculateTaxResponse = JsonConvert.DeserializeObject<TaxResponse>(responseData);

                if (calculateTaxResponse != null)
                {
                    return calculateTaxResponse;
                }

                return await Task.FromResult(default(TaxResponse));
            }
            catch (Exception exception)
            {
                throw new CalculateTaxResponseException(exception.Message);
            }
        }

        private static StringContent ConvertRequestToJsonHttpContent(object content) => new(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json");
    }
}
