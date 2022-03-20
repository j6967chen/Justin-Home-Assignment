using AutoMapper;
using TaxationService.Domain.Exceptions;
using TaxationService.Domain.Models;
using TaxationService.Domain.Models.TaxJarModel;
using TaxationService.Domain.Models.TaxServiceModel;

namespace TaxationService.Domain.ServiceCalculators
{
    public interface ITaxProxyService
    {
        Task<TaxForOrderResponse> CalculateTaxAsync(TaxForOrderRequest request, CancellationToken cancellationToken = default);

        Task<RateForLocationResponse> GetRatesForLocationAsync(TaxRateRequest request, CancellationToken cancellationToken = default);
    }

    public class TaxProxyService : ITaxProxyService
    {
        private readonly IEnumerable<ITaxCalculator> taxCalculators;
        private readonly IMapper mapper;

        public TaxProxyService(IEnumerable<ITaxCalculator> taxCalculators, IMapper mapper)
        { 
            this.taxCalculators = taxCalculators;
            this.mapper = mapper;
        }

        public async Task<RateForLocationResponse> GetRatesForLocationAsync(TaxRateRequest request, CancellationToken cancellationToken = default)
        {
            //Tax Service would need to decide which to use based on the Customer that is consuming the Tax Service.
            //We currently verify if the requested calculator type is TaxJar. 
            var taxJarCalculator = this.taxCalculators.FirstOrDefault(c => c.GetCalculatorType == request.CalculatorType);

            if (taxJarCalculator != null)
            {
                try
                {
                    //Map proxy service request contract to TaxJar rate request contract.
                    var taxJarRateRequest = this.mapper.Map<Rate>(request);

                    var response = await taxJarCalculator.GetRatesForLocationAsync(taxJarRateRequest, cancellationToken).ConfigureAwait(false);

                    if (response != null)
                    {
                        //TODO: will add more attributes if needed. This is for demo purpose.
                        return new RateForLocationResponse
                        {
                            Country = response.Rate.Country,
                            CombindedRate = response.Rate.CombinedRate
                        };
                    }
                }
                catch (Exception ex)
                {
                    throw new CalculateTaxRateResponseException(ex.Message);
                }
            }
            else
            {
                throw new CalculateTaxRateResponseException($"The custom calculator type is not found. request type: {request.CalculatorType.ToString()}");
            }

            return await Task.FromResult(default(RateForLocationResponse));
        }

        public async Task<TaxForOrderResponse> CalculateTaxAsync(TaxForOrderRequest request, CancellationToken cancellationToken)
        {

            //Tax Service would need to decide which to use based on the Customer that is consuming the Tax Service.
            //We currently verify if the requested calculator type is TaxJar. 
            var taxJarCalculator = this.taxCalculators.FirstOrDefault(c => c.GetCalculatorType == request.CalculatorType);

            if (taxJarCalculator != null)
            {
                //map client tax request to taxJar tax request.
                var tax = this.mapper.Map<Tax>(request);

                if (tax == null)
                {
                    throw new CalculateTaxForOrderRequestException();
                }

                var response = await taxJarCalculator.CalculateTaxAsync(tax, cancellationToken).ConfigureAwait(false);

                if (response != null)
                {
                    //TODO: will add more attributes if needed.
                    return new TaxForOrderResponse
                    {
                        TotalTax = response.Tax.AmountToCollect,
                        OrderTotalAmount = response.Tax.TaxableAmount,
                        TaxableShipping = response.Tax.Shipping
                    };
                }

                return await Task.FromResult<TaxForOrderResponse>(default); 
            }

            throw new CalculateTaxResponseException($"The tax calculator type is not supported. request type: {request.CalculatorType.ToString()}");
        }

    }
}
