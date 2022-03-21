using System.Text;
using System.Text.RegularExpressions;
using TaxationService.Domain.Models.TaxJarModel;

namespace TaxationService.Domain.ServiceCalculators.TaxCalculationClients
{
    public interface ITaxJarClient
    {
        Task<HttpResponseMessage> CalculateOrderTaxAsync(StringContent content, CancellationToken cancellationToken);
        Task<HttpResponseMessage> GetRatesAsync(Rate rate, CancellationToken cancellationToken);
    }

    public class TaxJarClient : ITaxJarClient
    {
        private const string usZipRegEx = @"^\d{5}(?:[-\s]\d{4})?$";

        private readonly IHttpClientFactory httpClientFactory;
        private HttpClient httpClient => this.httpClientFactory.CreateClient("TaxJar");

        public TaxJarClient(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<HttpResponseMessage> CalculateOrderTaxAsync(StringContent content, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(this.httpClient.BaseAddress + "v2/taxes"),
                Method = HttpMethod.Post,
                Content = content
            };

            return await this.httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        public async Task<HttpResponseMessage> GetRatesAsync(Rate rate, CancellationToken cancellationToken)
        {
            this.ValidateZipCode(rate);

            var urlParameters = TaxJarClient.BuildUrlParameter(rate);

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri($"{this.httpClient.BaseAddress}v2/rates/{urlParameters}"),
                Method = HttpMethod.Get,
            };

            return await this.httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        private static StringBuilder BuildUrlParameter(Rate rate)
        {
            var path = new StringBuilder(rate.Zip);

            bool isFirst = true;

            if (!string.IsNullOrEmpty(rate.Country))
            {
                if (rate.Country.Length != 2)
                {
                    throw new ArgumentException("Country Code lenght must be equal 2");
                }

                path.Append($"?country={rate.Country}");
                isFirst = false;
            }

            if (!string.IsNullOrEmpty(rate.State))
            {
                if (!isFirst)
                {
                    path.Append($"&state={rate.State}");
                }
                else
                {
                    path.Append($"?state={rate.State}");
                    isFirst = false;
                }
            }

            if (!string.IsNullOrEmpty(rate.City))
            {
                if (!isFirst)
                {
                    path.Append($"&city={rate.City}");
                }
                else
                {
                    path.Append($"?city={rate.City}");
                    isFirst = false;
                }
            }

            if (!string.IsNullOrEmpty(rate.Street))
            {
                if (!isFirst)
                {
                    path.Append($"&street={rate.Street}");
                }
                else
                {
                    path.Append($"?street={rate.Street}");
                }
            }

            return path;
        }
        
        private static bool IsUSZipCode(string zipCode) => !Regex.Match(zipCode, usZipRegEx).Success;

        private void ValidateZipCode(Rate rate)
        {
            if (string.IsNullOrEmpty(rate.Zip))
            {
                throw new ArgumentException("Zip code can't be empty.");
            }
            else
            {
                if (rate.Country == "US"
                        &&
                    IsUSZipCode(rate.Zip))
                {
                    throw new ArgumentException("Zip code format is incorrect.");
                }
            }
        }
    }
}
