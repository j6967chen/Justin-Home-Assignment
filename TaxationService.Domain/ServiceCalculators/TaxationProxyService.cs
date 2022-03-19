using AutoMapper;
using TaxationService.Domain.Exceptions;
using TaxationService.Domain.Models;
using TaxationService.Domain.Models.TaxJarModel;
using TaxationService.Domain.Models.TaxServiceModel;

namespace TaxationService.Domain.ServiceCalculators
{
    public interface ITaxationProxyService
    {
        Task<TaxForOrderResponse> CalculateTaxAsync(TaxForOrderRequest request, CancellationToken cancellationToken = default);

        Task<RateForLocation> GetRatesForLocationAsync(TaxRateRequest request, CancellationToken cancellationToken = default);
    }

    public class TaxationProxyService : ITaxationProxyService
    {
        private readonly IEnumerable<ITaxCalculator> taxCalculators;
        private readonly IMapper mapper;

        public TaxationProxyService(IEnumerable<ITaxCalculator> taxCalculators, IMapper mapper )
        { 
            this.taxCalculators = taxCalculators;
            this.mapper = mapper;
        }

        public async Task<RateForLocation> GetRatesForLocationAsync(TaxRateRequest request, CancellationToken cancellationToken = default)
        {
            //Tax Service would need to decide which to use based on the Customer that is consuming the Tax Service.
            //We currently verify if the requested calculator type is TaxJar. 
            var taxJarCalculator = this.taxCalculators.FirstOrDefault(c => c.GetCalculatorType == request.CalculatorType);

            if (taxJarCalculator != null)
            {
                try
                {
                    //Map service request contract to TaxJar rate request.
                    var taxJarRateRequest = this.mapper.Map<Rate>(request);

                    var response = await taxJarCalculator.GetRatesForLocationAsync(taxJarRateRequest, cancellationToken).ConfigureAwait(false);

                    if (response != null)
                    {
                        return new RateForLocation
                        {
                            Country = response.Rate.Country,
                            CombindRate = response.Rate.CombinedRate
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
                throw new CalculateTaxRateResponseException("The custom calculator type is not found.");
            }

            return await Task.FromResult(default(RateForLocation));
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

                var response = await taxJarCalculator.CalculateTaxAsync(tax, cancellationToken).ConfigureAwait(false);

                if (response != null)
                {
                    return new TaxForOrderResponse
                    {
                        TotalTax = response.Tax.AmountToCollect,
                        OrderTotalAmount = response.Tax.TaxableAmount,
                        TaxableShipping = response.Tax.Shipping
                    };
                }
            }

            return await Task.FromResult(default(TaxForOrderResponse));
        }

    }
}
