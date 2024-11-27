﻿using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using NUnit.Framework;
using ThamCoCheapestProductService.Dtos;
using ThamCoCheapestProductService.Services.CompanyProduct;

namespace ThamCoCheapestProductService.Tests
{
    [TestFixture]
    public class CompanyProductServiceFakeTests
    {
        private CompanyProductServiceFake _service;

        [SetUp]
        public void SetUp()
        {
            _service = new CompanyProductServiceFake();
        }

        [Test]
        public async Task GetCompanyProduct_ReturnsCorrectProduct()
        {
            // Arrange
            int companyId = 1;
            int productId = 1;

            // Act
            var response = await _service.GetCompanyProduct(companyId, productId);
            var content = await response.Content.ReadAsStringAsync();
            var product = JsonSerializer.Deserialize<CompanyProductsDto>(content);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(product);
            Assert.AreEqual(companyId, product.CompanyId);
            Assert.AreEqual(productId, product.ProductId);
        }

        [Test]
        public async Task GetCompanyProduct_ReturnsNullForInvalidProduct()
        {
            // Arrange
            int companyId = 1;
            int productId = 999; // Invalid productId

            // Act
            var response = await _service.GetCompanyProduct(companyId, productId);
            var content = await response.Content.ReadAsStringAsync();
            var product = JsonSerializer.Deserialize<CompanyProductsDto>(content);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.Null(product);
        }

        [Test]
        public async Task GetCompanyProducts_ReturnsAllProducts()
        {
            // Act
            var response = await _service.GetCompanyProducts();
            var content = await response.Content.ReadAsStringAsync();
            var products = JsonSerializer.Deserialize<IEnumerable<CompanyProductsDto>>(content);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(products);
            Assert.AreEqual(10, products.Count());
        }
    }
}
