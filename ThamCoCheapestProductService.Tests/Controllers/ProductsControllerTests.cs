using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using ThamCoCheapestProductService.Controllers;
using ThamCoCheapestProductService.Dtos;
using ThamCoCheapestProductService.Services;
using Microsoft.AspNetCore.Mvc;

namespace ThamCoCheapestProductService.Tests.Controllers
{
    [TestFixture]
    public class ProductsControllerTests
    {
        private Mock<ICompanyProductService> _companyProductServiceMock;
        private Mock<IProductService> _productServiceMock;
        private Mock<IMapper> _mapperMock;
        private Mock<ILogger<ProductsController>> _loggerMock;
        private ProductsController _controller;

        [SetUp]
        public void SetUp()
        {
            _companyProductServiceMock = new Mock<ICompanyProductService>();
            _productServiceMock = new Mock<IProductService>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<ProductsController>>();
            _controller = new ProductsController(_companyProductServiceMock.Object, _productServiceMock.Object, _mapperMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task GetAllCheapestProductSuppliers_ReturnsOkResult_WithListOfSuppliers()
        {
            // Arrange
            var suppliers = new List<CompanyWithProductDto>
            {
                new CompanyWithProductDto { ProductId = 1, CompanyId = 1, Name = "Product1", Brand = "Brand1", Description = "Description1", Price = 10, ImageUrl = "http://example.com/image1" },
                new CompanyWithProductDto { ProductId = 2, CompanyId = 2, Name = "Product2", Brand = "Brand2", Description = "Description2", Price = 20, ImageUrl = "http://example.com/image2" }
            };

            _productServiceMock.Setup(s => s.GetProducts()).ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new List<ProductDto>
                {
                    new ProductDto { ProductId = 1, Name = "Product1", Brand = "Brand1", Description = "Description1", ImageUrl = "http://example.com/image1" },
                    new ProductDto { ProductId = 2, Name = "Product2", Brand = "Brand2", Description = "Description2", ImageUrl = "http://example.com/image2" }
                }))
            });

            _companyProductServiceMock.Setup(s => s.GetCompanyProducts()).ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new List<CompanyProductsDto>
                {
                    new CompanyProductsDto { ProductId = 1, CompanyId = 1, Price = 10 },
                    new CompanyProductsDto { ProductId = 2, CompanyId = 2, Price = 20 }
                }))
            });

            // Act
            var result = await _controller.GetAllCheapestProductSuppliers();

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.IsInstanceOf<IEnumerable<CompanyWithProductDto>>(okResult.Value);
            var returnedSuppliers = okResult.Value as IEnumerable<CompanyWithProductDto>;
            Assert.AreEqual(2, returnedSuppliers.Count());
        }

        [Test]
        public async Task GetAllCheapestProductSuppliers_ReturnsNotFound_WhenNoSuppliersFound()
        {
            // Arrange
            _productServiceMock.Setup(s => s.GetProducts()).ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new List<ProductDto>()))
            });

            // Act
            var result = await _controller.GetAllCheapestProductSuppliers();

            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result.Result);
            var notFoundResult = result.Result as NotFoundObjectResult;
            Assert.AreEqual("No suppliers found.", notFoundResult.Value);
        }

        [Test]
        public async Task GetCheapestProductSupplierById_ReturnsOkResult_WithSupplier()
        {
            // Arrange
            int productId = 1;
            var supplier = new CompanyWithProductDto { ProductId = productId, CompanyId = 1, Name = "Product1", Brand = "Brand1", Description = "Description1", Price = 10, ImageUrl = "http://example.com/image1" };

            _productServiceMock.Setup(s => s.GetProducts()).ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new List<ProductDto>
                {
                    new ProductDto { ProductId = 1, Name = "Product1", Brand = "Brand1", Description = "Description1", ImageUrl = "http://example.com/image1" }
                }))
            });

            _companyProductServiceMock.Setup(s => s.GetCompanyProducts()).ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new List<CompanyProductsDto>
                {
                    new CompanyProductsDto { ProductId = 1, CompanyId = 1, Price = 10 }
                }))
            });

            // Act
            var result = await _controller.GetCheapestProductSupplierById(productId);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.IsInstanceOf<CompanyWithProductDto>(okResult.Value);
            var returnedSupplier = okResult.Value as CompanyWithProductDto;
            Assert.AreEqual(productId, returnedSupplier.ProductId);
        }

        [Test]
        public async Task GetCheapestProductSupplierById_ReturnsNotFound_WhenNoSupplierFound()
        {
            // Arrange
            int productId = 999; // Invalid productId

            _productServiceMock.Setup(s => s.GetProducts()).ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new List<ProductDto>()))
            });

            // Act
            var result = await _controller.GetCheapestProductSupplierById(productId);

            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result.Result);
            var notFoundResult = result.Result as NotFoundObjectResult;
            Assert.AreEqual("No supplier found for the specified product ID.", notFoundResult.Value);
        }
    }
}
