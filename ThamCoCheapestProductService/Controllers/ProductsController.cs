using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ThamCoCheapestProductService.Dtos;
using ThamCoCheapestProductService.Services;

namespace ThamCoCheapestProductService.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ICompanyProductService _companyProductService;
        private readonly IProductService _productService;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(ICompanyProductService companyProductService, IProductService productService, IMapper mapper, ILogger<ProductsController> logger)
        {
            _companyProductService = companyProductService;
            _productService = productService;
            _mapper = mapper;
            _logger = logger;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CompanyWithProductDto>>> GetAllCheapestProductSuppliers()
        {
            try
            {
                var cheapestProductSuppliers = await FindAllCheapestProductSuppliers();
                if (cheapestProductSuppliers == null || !cheapestProductSuppliers.Any())
                {
                    return NotFound("No suppliers found.");
                }

                return Ok(cheapestProductSuppliers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all cheapest product suppliers.");
                return StatusCode(500, "Internal server error");
            }
        }

        //[Authorize]
        [HttpGet("{productId}")]
        public async Task<ActionResult<CompanyWithProductDto>> GetCheapestProductSupplierById(int productId)
        {
            try
            {
                var cheapestProductSupplier = await FindCheapestProductSupplierById(productId);
                if (cheapestProductSupplier == null)
                {
                    return NotFound("No supplier found for the specified product ID.");
                }

                return Ok(cheapestProductSupplier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while getting the cheapest product supplier for product ID {productId}.");
                return StatusCode(500, "Internal server error");
            }
        }

        private async Task<IEnumerable<CompanyWithProductDto>> FindAllCheapestProductSuppliers()
        {
            var productResponse = await _productService.GetProducts();
            if (!productResponse.IsSuccessStatusCode)
            {
                throw new Exception(productResponse.ReasonPhrase);
            }
            if (productResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var contentString = await productResponse.Content.ReadAsStringAsync();
            var products = JsonSerializer.Deserialize<List<ProductDto>>(contentString, options);

            if (products == null || !products.Any())
            {
                return null;
            }
            var companyProductResponse = new HttpResponseMessage();
            try
            {
                 companyProductResponse = await _companyProductService.GetCompanyProducts();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all cheapest product suppliers.");
            }
            if (!companyProductResponse.IsSuccessStatusCode)
            {
                throw new Exception(companyProductResponse.ReasonPhrase);
            }

            contentString = await companyProductResponse.Content.ReadAsStringAsync();
            var companyProducts = JsonSerializer.Deserialize<List<CompanyProductsDto>>(contentString, options);

            var cheapestProductSuppliers = new List<CompanyWithProductDto>();

            foreach (var product in products)
            {
                var cheapestCompanyProduct = companyProducts
                    .Where(cp => cp.ProductId == product.ProductId)
                    .OrderBy(cp => cp.Price)
                    .FirstOrDefault();

                if (cheapestCompanyProduct != null)
                {
                    var companyWithProductDto = new CompanyWithProductDto
                    {
                        ProductId = product.ProductId,
                        CompanyId = cheapestCompanyProduct.CompanyId,
                        Name = product.Name,
                        Brand = product.Brand,
                        Description = product.Description,
                        Price = cheapestCompanyProduct.Price,
                        ImageUrl = product.ImageUrl,
                        StockLevel = cheapestCompanyProduct.StockLevel
                    };

                    cheapestProductSuppliers.Add(companyWithProductDto);
                }
            }

            return cheapestProductSuppliers;
        }

        private async Task<CompanyWithProductDto> FindCheapestProductSupplierById(int productId)
        {
            var productResponse = await _productService.GetProducts();
            if (!productResponse.IsSuccessStatusCode)
            {
                throw new Exception(productResponse.ReasonPhrase);
            }
            if (productResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var contentString = await productResponse.Content.ReadAsStringAsync();
            var products = JsonSerializer.Deserialize<List<ProductDto>>(contentString, options);

            if (products == null || !products.Any())
            {
                return null;
            }

            var product = products.FirstOrDefault(p => p.ProductId == productId);
            if (product == null)
            {
                return null;
            }

            var companyProductResponse = await _companyProductService.GetCompanyProducts();
            if (!companyProductResponse.IsSuccessStatusCode)
            {
                throw new Exception(companyProductResponse.ReasonPhrase);
            }

            contentString = await companyProductResponse.Content.ReadAsStringAsync();
            var companyProducts = JsonSerializer.Deserialize<List<CompanyProductsDto>>(contentString, options);

            var cheapestCompanyProduct = companyProducts
                .Where(cp => cp.ProductId == product.ProductId)
                .OrderBy(cp => cp.Price)
                .FirstOrDefault();

            if (cheapestCompanyProduct == null)
            {
                return null;
            }

            var companyWithProductDto = new CompanyWithProductDto
            {
                ProductId = product.ProductId,
                CompanyId = cheapestCompanyProduct.CompanyId,
                Name = product.Name,
                Brand = product.Brand,
                Description = product.Description,
                Price = cheapestCompanyProduct.Price,
                ImageUrl = product.ImageUrl,
                StockLevel = cheapestCompanyProduct.StockLevel
            };

            return companyWithProductDto;
        }
    }
}
