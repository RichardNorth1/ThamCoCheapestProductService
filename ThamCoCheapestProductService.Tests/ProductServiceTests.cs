using Moq;
using NUnit.Framework;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using ThamCoCheapestProductService.Services;
using ThamCoCheapestProductService.Services.Token;
using ThamCoCheapestProductService.Dtos;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http.Headers;
using Moq.Protected;

namespace ThamCoCheapestProductService.Tests
{
    [TestFixture]
    public class ProductServiceTests
    {
        private Mock<IMemoryCache> _cacheMock;
        private Mock<IHttpClientFactory> _httpClientFactoryMock;
        private Mock<IConfiguration> _configurationMock;
        private Mock<ITokenService> _tokenServiceMock;

        [SetUp]
        public void SetUp()
        {
            _cacheMock = new Mock<IMemoryCache>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _configurationMock = new Mock<IConfiguration>();
            _tokenServiceMock = new Mock<ITokenService>();
        }

        private ProductService CreateProductService(HttpClient httpClient)
        {
            return new ProductService(
                httpClient,
                _cacheMock.Object,
                _httpClientFactoryMock.Object,
                _configurationMock.Object,
                _tokenServiceMock.Object
            );
        }

        [Test]
        public async Task GetProduct_ShouldReturnCachedResponse_WhenCacheIsAvailable()
        {
            // Arrange
            var productId = 1;
            var cachedResponse = new HttpResponseMessage(HttpStatusCode.OK);
            var cacheEntry = (object)cachedResponse;
            _cacheMock.Setup(c => c.TryGetValue($"Product_{productId}", out cacheEntry)).Returns(true);

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://example.com")
            };
            var service = CreateProductService(httpClient);

            // Act
            var response = await service.GetProduct(productId);

            // Assert
            Assert.That(response, Is.EqualTo(cachedResponse));
            _cacheMock.Verify(c => c.TryGetValue($"Product_{productId}", out cacheEntry), Times.Once);
        }

        [Test]
        public async Task GetProduct_ShouldReturnResponse_WhenCacheIsNotAvailable()
        {
            // Arrange
            var productId = 1;
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            var cacheEntry = (object)null;
            _cacheMock.Setup(c => c.TryGetValue($"Product_{productId}", out cacheEntry)).Returns(false);

            var mockCacheEntry = new Mock<ICacheEntry>();
            _cacheMock.Setup(c => c.CreateEntry(It.IsAny<object>())).Returns(mockCacheEntry.Object);

            _tokenServiceMock.Setup(t => t.GetToken()).ReturnsAsync(new TokenDto { AccessToken = "test_token" });

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(responseMessage);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://example.com")
            };
            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var service = CreateProductService(httpClient);

            // Act
            var response = await service.GetProduct(productId);

            // Assert
            Assert.That(response, Is.EqualTo(responseMessage));
            mockCacheEntry.VerifySet(entry => entry.Value = responseMessage, Times.Once);
        }

        [Test]
        public async Task GetProducts_ShouldReturnCachedResponse_WhenCacheIsAvailable()
        {
            // Arrange
            var cachedResponse = new HttpResponseMessage(HttpStatusCode.OK);
            var cacheEntry = (object)cachedResponse;
            _cacheMock.Setup(c => c.TryGetValue("AllProducts", out cacheEntry)).Returns(true);

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://example.com")
            };
            var service = CreateProductService(httpClient);

            // Act
            var response = await service.GetProducts();

            // Assert
            Assert.That(response, Is.EqualTo(cachedResponse));
        }

        [Test]
        public async Task GetProducts_ShouldReturnResponse_WhenCacheIsNotAvailable()
        {
            // Arrange
            var token = new TokenDto { AccessToken = "test_token" };
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            var cacheEntry = (object)null;
            _cacheMock.Setup(c => c.TryGetValue("AllProducts", out cacheEntry)).Returns(false);
            _tokenServiceMock.Setup(t => t.GetToken()).ReturnsAsync(token);

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(responseMessage);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://example.com")
            };
            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var mockCacheEntry = new Mock<ICacheEntry>();
            _cacheMock.Setup(c => c.CreateEntry(It.IsAny<object>())).Returns(mockCacheEntry.Object);

            var service = CreateProductService(httpClient);

            // Act
            var response = await service.GetProducts();

            // Assert
            Assert.That(response, Is.EqualTo(responseMessage));
            mockCacheEntry.VerifySet(entry => entry.Value = responseMessage, Times.Once);
        }
    }
}
