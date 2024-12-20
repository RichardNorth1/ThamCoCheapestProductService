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
        private readonly ITokenService _tokenService;

        public CompanyProductService(HttpClient httpClient, 
            IMemoryCache cache, 
            ITokenService tokenService
            )
        {
            _httpClient = httpClient;
            _cache = cache;
            _tokenService = tokenService;

        }

        public async Task<HttpResponseMessage> GetCompanyProduct(int companyId, int productId)
        {
            if (_cache.TryGetValue($"CompanyProduct_{productId}", out HttpResponseMessage? cachedResponse))
            {
                return cachedResponse ?? new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (await _tokenService.GetToken()).AccessToken);
            var response = await _httpClient.GetAsync($"/api/companyProducts/{companyId}/{productId}");

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

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenService.GetToken().Result.AccessToken);

            var response = await _httpClient.GetAsync($"/api/companyProducts");

            if (response.IsSuccessStatusCode)
            {
                _cache.Set("AllCompanyProducts", response, TimeSpan.FromMinutes(5));
            }
            return response;
        }
    }
}