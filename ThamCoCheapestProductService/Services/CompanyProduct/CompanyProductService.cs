using Microsoft.Extensions.Caching.Memory;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly;

namespace ThamCoCheapestProductService.Services
{
    public class CompanyProductService: ICompanyProductService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
        private readonly AsyncCircuitBreakerPolicy<HttpResponseMessage> _circuitBreakerPolicy;

        public CompanyProductService(HttpClient httpClient, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _cache = cache;

            _retryPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .RetryAsync(6, onRetry: (response, retryCount) =>
                {
                });

            _circuitBreakerPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .CircuitBreakerAsync(5, TimeSpan.FromSeconds(5));
        }

        public async Task<HttpResponseMessage> GetCompanyProduct(int companyId, int productId)
        {
            if (_cache.TryGetValue($"CompanyProduct_{productId}", out HttpResponseMessage cachedResponse))
            {
                return cachedResponse;
            }

            var response = await _retryPolicy.ExecuteAsync(() =>
                _circuitBreakerPolicy.ExecuteAsync(() =>
                    _httpClient.GetAsync($"/api/companyProducts/{companyId}/{productId}")));

            if (response.IsSuccessStatusCode)
            {
                _cache.Set($"CompanyProduct_{productId}", response, TimeSpan.FromMinutes(5));
            }
            return response;
        }

        public async Task<HttpResponseMessage> GetCompanyProducts()
        {
            if (_cache.TryGetValue("AllCompanyProducts", out HttpResponseMessage cachedResponse))
            {
                return cachedResponse;
            }

            var response = await _retryPolicy.ExecuteAsync(() =>
                _circuitBreakerPolicy.ExecuteAsync(() =>
                    _httpClient.GetAsync($"/api/companyProducts")));

            if (response.IsSuccessStatusCode)
            {
                _cache.Set("AllCompanyProducts", response, TimeSpan.FromMinutes(5));
            }
            return response;
        }
    }
}