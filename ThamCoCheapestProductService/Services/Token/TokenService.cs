using Microsoft.Extensions.Caching.Memory;
using System.Net.Http;
using System.Text.Json;
using ThamCoCheapestProductService.Dtos;

namespace ThamCoCheapestProductService.Services.Token
{
    public class TokenService : ITokenService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TokenService> _logger;
        private readonly IMemoryCache _cache;

        public TokenService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<TokenService> logger, IMemoryCache cache)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _cache = cache;
        }

        public async Task<TokenDto> GetToken()
        {
            if (_cache.TryGetValue("Token", out TokenDto token))
            {
                return token;
            }

            var tokenClient = _httpClientFactory.CreateClient();
            tokenClient.BaseAddress = new Uri(_configuration["Auth:ProductService:Authority"]);
            var tokenParams = new Dictionary<string, string>
                {
                    {"grant_type", "client_credentials"},
                    {"client_id", _configuration["Auth:ProductService:ClientId"]},
                    {"client_secret", _configuration["Auth:ProductService:ClientSecret"]},
                    {"audience", _configuration["Auth:ProductService:Audience"]}
                };
            var tokenForm = new FormUrlEncodedContent(tokenParams);
            var tokenResponse = await tokenClient.PostAsync("oauth/token", tokenForm);

            var contentString = await tokenResponse.Content.ReadAsStringAsync();
            try
            {
                token = JsonSerializer.Deserialize<TokenDto>(contentString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (token == null || string.IsNullOrEmpty(token.AccessToken))
                {
                    throw new Exception("Token deserialization failed or access token is null/empty.");
                }

                _cache.Set("Token", token, TimeSpan.FromMinutes(60));
            }
            catch (Exception ex)
            {
                throw;
            }

            return token;
        }
    }
}
