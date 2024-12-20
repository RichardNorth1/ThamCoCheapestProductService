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


        public ProductService(HttpClient httpClient, 
            IMemoryCache cache,   
            ITokenService tokenService)
        {
            _httpClient = httpClient;

            _cache = cache;
            _tokenService = tokenService;
        }

        public async Task<HttpResponseMessage> GetProduct(int productId)
        {
            if (_cache.TryGetValue($"Product_{productId}", out HttpResponseMessage cachedResponse))
            {
                return cachedResponse;
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenService.GetToken().Result.AccessToken);
            var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"/api/products/{productId}"));


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

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenService.GetToken().Result.AccessToken);
            var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/api/products"));


            if (response.IsSuccessStatusCode)
            {
                _cache.Set("AllProducts", response, TimeSpan.FromMinutes(5));
            }
            return response;
        }
    }
}
