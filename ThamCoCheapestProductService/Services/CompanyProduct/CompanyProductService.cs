using Microsoft.Extensions.Caching.Memory;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly;
using ThamCoCheapestProductService.Dtos;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ThamCoCheapestProductService.Services.Token;

namespace ThamCoCheapestProductService.Services
{
    public class CompanyProductService : ICompanyProductService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
        private readonly AsyncCircuitBreakerPolicy<HttpResponseMessage> _circuitBreakerPolicy;
        private readonly ILogger<CompanyProductService> _logger;

        public CompanyProductService(HttpClient httpClient, 
            IMemoryCache cache, 
            IHttpClientFactory httpClientFactory, 
            IConfiguration configuration, 
            ILogger<CompanyProductService> logger,
            ITokenService tokenService
            )
        {
            _httpClient = httpClient;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _cache = cache;
            _logger = logger;
            _tokenService = tokenService;

            _retryPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .RetryAsync(6, onRetry: (response, retryCount) =>
                {
                });

            _circuitBreakerPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .CircuitBreakerAsync(5, TimeSpan.FromSeconds(5));
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<HttpResponseMessage> GetCompanyProduct(int companyId, int productId)
        {
            if (_cache.TryGetValue($"CompanyProduct_{productId}", out HttpResponseMessage? cachedResponse))
            {
                return cachedResponse;
            }

            var response = await _retryPolicy.ExecuteAsync(() =>
                _circuitBreakerPolicy.ExecuteAsync(() =>
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenService.GetToken().Result.AccessToken);
                    return _httpClient.GetAsync($"/api/companyProducts/{companyId}/{productId}");
                }));

            if (response.IsSuccessStatusCode)
            {
                _cache.Set($"CompanyProduct_{productId}", response, TimeSpan.FromMinutes(5));
            }
            return response;
        }

        public async Task<HttpResponseMessage> GetCompanyProducts()
        {
            if (_cache.TryGetValue("AllCompanyProducts", out HttpResponseMessage? cachedResponse))
            {
                return cachedResponse;
            }

            var response = await _retryPolicy.ExecuteAsync(() =>
                _circuitBreakerPolicy.ExecuteAsync(() =>
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenService.GetToken().Result.AccessToken);
                    return _httpClient.GetAsync($"/api/companyProducts");
                }
                    ));

            if (response.IsSuccessStatusCode)
            {
                _cache.Set("AllCompanyProducts", response, TimeSpan.FromMinutes(5));
            }
            return response;
        }
    }
}