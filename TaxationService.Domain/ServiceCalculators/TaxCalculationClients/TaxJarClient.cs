using System.Net.Http.Headers;
using System.Text;
using TaxationService.Domain.Configurations;
using TaxationService.Domain.Models.TaxJarModel;

namespace TaxationService.Domain.ServiceCalculators.TaxCalculationClients
{
    public interface ITaxJarClient
    {
        Task<HttpResponseMessage> PostTaxAsync(StringContent content, CancellationToken cancellationToken);
        Task<HttpResponseMessage> GetRatesAsync(Rate rate, CancellationToken cancellationToken);
    }

    public class TaxJarClient : ITaxJarClient
    {
        private readonly TaxJarConfiguration configurationSettings;

        private static HttpClient HttpClient;

        public TaxJarClient(HttpClient httpClient, TaxJarConfiguration taxJarSettings)
        {
            TaxJarClient.HttpClient = httpClient;
            this.configurationSettings = taxJarSettings;
        }

        public async Task<HttpResponseMessage> PostTaxAsync(StringContent content, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(this.configurationSettings.ApiBaseUrl)
                    ||
                string.IsNullOrEmpty(this.configurationSettings.ApiVersion)
                    ||
                string.IsNullOrEmpty(this.configurationSettings.ApiKey))
            {
                throw new ArgumentException("One or more of configuration settings are not defined.");
            }

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(configurationSettings.ApiBaseUrl + "/v2/taxes"),
                Method = HttpMethod.Post,
                Content = content
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.configurationSettings.ApiKey);

            request.Headers.Add("x-api-version", this.configurationSettings.ApiVersion);

            return await TaxJarClient.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        public async Task<HttpResponseMessage> GetRatesAsync(Rate rate, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(this.configurationSettings.ApiBaseUrl)
                        ||
                    string.IsNullOrEmpty(this.configurationSettings.ApiVersion)
                        ||
                    string.IsNullOrEmpty(this.configurationSettings.ApiKey))
            {
                throw new ArgumentException("One or more of configuration settings are not defined.");
            }

            if (string.IsNullOrEmpty(rate.Zip))
            {
                throw new ArgumentException("Zip code can't be empty.");
            }

            var path = new StringBuilder(rate.Zip);

            bool isFirst = true;

            if (!string.IsNullOrEmpty(rate.Country))
            {
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

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri($"{configurationSettings.ApiBaseUrl}/v2/rates/{path}"),
                Method = HttpMethod.Get,
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.configurationSettings.ApiKey);

            request.Headers.Add("x-api-version", this.configurationSettings.ApiVersion);

            return await TaxJarClient.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
