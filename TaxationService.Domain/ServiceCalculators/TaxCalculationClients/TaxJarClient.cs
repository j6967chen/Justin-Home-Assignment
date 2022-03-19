using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using TaxationService.Domain.Configurations;
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
        private readonly TaxJarConfiguration configurationSettings;
        private string usZipRegEx = @"^\d{5}(?:[-\s]\d{4})?$";

        private readonly HttpClient httpClient;

        public TaxJarClient(HttpClient httpClient, TaxJarConfiguration taxJarSettings)
        {
            this.httpClient = httpClient;
            this.configurationSettings = taxJarSettings;
        }

        public async Task<HttpResponseMessage> CalculateOrderTaxAsync(StringContent content, CancellationToken cancellationToken)
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

            return await this.httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        public async Task<HttpResponseMessage> GetRatesAsync(Rate rate, CancellationToken cancellationToken)
        {
            this.ValidateSettingsAndParameters(rate);

            var urlParameter = TaxJarClient.BuildUrlParameter(rate);

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri($"{configurationSettings.ApiBaseUrl}/v2/rates/{urlParameter}"),
                Method = HttpMethod.Get,
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.configurationSettings.ApiKey);

            request.Headers.Add("x-api-version", this.configurationSettings.ApiVersion);

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
        
        private bool IsUSZipCode(string zipCode) => !Regex.Match(zipCode, usZipRegEx).Success;

        private void ValidateSettingsAndParameters(Rate rate)
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
