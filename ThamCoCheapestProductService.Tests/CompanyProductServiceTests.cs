using Moq;
using Moq.Protected;
using NUnit.Framework;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ThamCoCheapestProductService.Services;
using ThamCoCheapestProductService.Services.Token;
using ThamCoCheapestProductService.Dtos;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using System.Threading;

namespace ThamCoCheapestProductService.Tests
{
    [TestFixture]
    public class CompanyProductServiceTests
    {
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private Mock<IMemoryCache> _cacheMock;
        private Mock<IHttpClientFactory> _httpClientFactoryMock;
        private Mock<IConfiguration> _configurationMock;
        private Mock<ILogger<CompanyProductService>> _loggerMock;
        private Mock<ITokenService> _tokenServiceMock;
        private CompanyProductService _service;
        private HttpClient _httpClient;

        [SetUp]
        public void SetUp()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _cacheMock = new Mock<IMemoryCache>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _configurationMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<CompanyProductService>>();
            _tokenServiceMock = new Mock<ITokenService>();

            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://example.com") // Set a valid base address
            };
            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(_httpClient);

            _service = new CompanyProductService(
                _httpClient,
                _cacheMock.Object,
                _httpClientFactoryMock.Object,
                _configurationMock.Object,
                _loggerMock.Object,
                _tokenServiceMock.Object
            );
        }

        [TearDown]
        public void TearDown()
        {
            _httpClient.Dispose();
        }

        [Test]
        public async Task GetCompanyProduct_ShouldReturnCachedResponse_WhenCacheIsAvailable()
        {
            // Arrange
            var companyId = 1;
            var productId = 1;
            var cachedResponse = new HttpResponseMessage(HttpStatusCode.OK);
            object cacheEntry = cachedResponse;
            _cacheMock.Setup(c => c.TryGetValue($"CompanyProduct_{productId}", out cacheEntry)).Returns(true);

            // Act
            var response = await _service.GetCompanyProduct(companyId, productId);

            // Assert
            Assert.That(response, Is.EqualTo(cachedResponse));
        }

        [Test]
        public async Task GetCompanyProduct_ShouldReturnResponse_WhenCacheIsNotAvailable()
        {
            // Arrange
            var companyId = 1;
            var productId = 1;
            var token = new TokenDto { AccessToken = "test_token" };
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            object cacheEntry = null;
            _cacheMock.Setup(c => c.TryGetValue($"CompanyProduct_{productId}", out cacheEntry)).Returns(false);

            var mockCacheEntry = new Mock<ICacheEntry>();
            _cacheMock.Setup(c => c.CreateEntry(It.IsAny<object>())).Returns(mockCacheEntry.Object);

            _tokenServiceMock.Setup(t => t.GetToken()).ReturnsAsync(token);
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(responseMessage);

            // Act
            var response = await _service.GetCompanyProduct(companyId, productId);

            // Assert
            Assert.That(response, Is.EqualTo(responseMessage));
            mockCacheEntry.VerifySet(entry => entry.Value = responseMessage, Times.Once);
        }



        [Test]
        public async Task GetCompanyProducts_ShouldReturnCachedResponse_WhenCacheIsAvailable()
        {
            // Arrange
            var cachedResponse = new HttpResponseMessage(HttpStatusCode.OK);
            object cacheEntry = cachedResponse;
            _cacheMock.Setup(c => c.TryGetValue("AllCompanyProducts", out cacheEntry)).Returns(true);

            // Act
            var response = await _service.GetCompanyProducts();

            // Assert
            Assert.That(response, Is.EqualTo(cachedResponse));
        }
        [Test]
        public async Task GetCompanyProducts_ShouldReturnResponse_WhenCacheIsNotAvailable()
        {
            // Arrange
            var token = new TokenDto { AccessToken = "test_token" };
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            object cacheEntry = null;
            _cacheMock.Setup(c => c.TryGetValue("AllCompanyProducts", out cacheEntry)).Returns(false);

            var mockCacheEntry = new Mock<ICacheEntry>();
            _cacheMock.Setup(c => c.CreateEntry(It.IsAny<object>())).Returns(mockCacheEntry.Object);

            _tokenServiceMock.Setup(t => t.GetToken()).ReturnsAsync(token);
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(responseMessage);

            // Act
            var response = await _service.GetCompanyProducts();

            // Assert
            Assert.That(response, Is.EqualTo(responseMessage));
            mockCacheEntry.VerifySet(entry => entry.Value = responseMessage, Times.Once);
        }


    }
}
