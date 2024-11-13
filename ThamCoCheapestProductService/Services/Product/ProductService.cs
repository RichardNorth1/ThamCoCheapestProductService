using Microsoft.Extensions.Caching.Memory;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace ThamCoCheapestProductService.Services
{
    public class ProductService : IProductService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
        private readonly AsyncCircuitBreakerPolicy<HttpResponseMessage> _circuitBreakerPolicy;

        public ProductService(HttpClient httpClient, IMemoryCache cache)
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

        public async Task<HttpResponseMessage> GetProduct(int productId)
        {
            if (_cache.TryGetValue($"Product_{productId}", out HttpResponseMessage cachedResponse))
            {
                return cachedResponse;
            }

            var response = await _retryPolicy.ExecuteAsync(() =>
                _circuitBreakerPolicy.ExecuteAsync(() =>
                    _httpClient.GetAsync($"/api/products/{productId}")));

            if (response.IsSuccessStatusCode)
            {
                _cache.Set($"Product_{productId}", response, TimeSpan.FromMinutes(5));
            }
            return response;
        }

        public async Task<HttpResponseMessage> GetProducts()
        {
            if (_cache.TryGetValue("AllProducts", out HttpResponseMessage cachedResponse))
            {
                return cachedResponse;
            }

            var response = await _retryPolicy.ExecuteAsync(() =>
                _circuitBreakerPolicy.ExecuteAsync(() =>
                    _httpClient.GetAsync($"/api/products")));

            if (response.IsSuccessStatusCode)
            {
                _cache.Set("AllProducts", response, TimeSpan.FromMinutes(5));
            }
            return response;
        }
    }
}
