using Microsoft.Extensions.Caching.Memory;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using ThamCoCheapestProductService.Dtos;
using ThamCoCheapestProductService.Services.Token;

namespace ThamCoCheapestProductService.Services
{
    public class ProductService : IProductService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ITokenService _tokenService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
        private readonly AsyncCircuitBreakerPolicy<HttpResponseMessage> _circuitBreakerPolicy;

        public ProductService(HttpClient httpClient, 
            IMemoryCache cache, 
            IHttpClientFactory httpClientFactory, 
            IConfiguration configuration,  
            ITokenService tokenService)
        {
            _httpClient = httpClient;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _cache = cache;
            _tokenService = tokenService;

            _retryPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .RetryAsync(6);

            _circuitBreakerPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
        }

        public async Task<HttpResponseMessage> GetProduct(int productId)
        {
            if (_cache.TryGetValue($"Product_{productId}", out HttpResponseMessage cachedResponse))
            {
                return cachedResponse;
            }

            var response = await _retryPolicy.ExecuteAsync(() =>
                _circuitBreakerPolicy.ExecuteAsync(async () =>
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenService.GetToken().Result.AccessToken);
                    var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"/api/products/{productId}"));
                    return response;
                }));

            if (response.IsSuccessStatusCode)
            {
                _cache.Set($"Product_{productId}", response, TimeSpan.FromMinutes(5));
            }
            return response;
        }

        public async Task<HttpResponseMessage> GetProducts()
        {

            // Check cache
            if (_cache.TryGetValue("AllProducts", out HttpResponseMessage cachedResponse))
            {
                return cachedResponse;
            }
            var response = await _retryPolicy.ExecuteAsync(() =>
                _circuitBreakerPolicy.ExecuteAsync(async () =>
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenService.GetToken().Result.AccessToken);
                    var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/api/products"));
                    return response;
                }));

            if (response.IsSuccessStatusCode)
            {
                _cache.Set("AllProducts", response, TimeSpan.FromMinutes(5));
            }
            return response;
        }
    }
}
