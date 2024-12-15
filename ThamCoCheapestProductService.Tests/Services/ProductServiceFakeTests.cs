using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using NUnit.Framework;
using ThamCoCheapestProductService.Dtos;
using ThamCoCheapestProductService.Services.Product;

namespace ThamCoCheapestProductService.Tests.Services
{
    [TestFixture]
    public class ProductServiceFakeTests
    {
        private ProductServiceFake _service;

        [SetUp]
        public void SetUp()
        {
            _service = new ProductServiceFake();
        }

        [Test]
        public async Task GetProduct_ReturnsCorrectProduct()
        {
            int productId = 1;

            var response = await _service.GetProduct(productId);
            var content = await response.Content.ReadAsStringAsync();
            var product = JsonSerializer.Deserialize<ProductDto>(content);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(product);
            Assert.AreEqual(productId, product.ProductId);
        }

        [Test]
        public async Task GetProduct_ReturnsNullForInvalidProduct()
        {
            int productId = 999; // Invalid productId

            var response = await _service.GetProduct(productId);
            var content = await response.Content.ReadAsStringAsync();
            var product = JsonSerializer.Deserialize<ProductDto>(content);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.Null(product);
        }

        [Test]
        public async Task GetProducts_ReturnsAllProducts()
        {
            var response = await _service.GetProducts();
            var content = await response.Content.ReadAsStringAsync();
            var products = JsonSerializer.Deserialize<IEnumerable<ProductDto>>(content);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(products);
            Assert.AreEqual(5, products.Count());
        }
    }
}
