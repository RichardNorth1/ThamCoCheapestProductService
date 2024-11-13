using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using ThamCoCheapestProductService.Dtos;
using ThamCoCheapestProductService.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ThamCoCheapestProductService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ICompanyProductService _companyProductService;
        private readonly IProductService _productService;
        private readonly IMapper _mapper;

        public ProductsController(ICompanyProductService companyProductService, IProductService productService, IMapper mapper)
        {
            _companyProductService = companyProductService;
            _productService = productService;
            _mapper = mapper;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CompanyWithProductDto>>> GetAllCheapestProductSuppliers()
        {
            try
            {
                Trace.WriteLine("Getting all cheapest product suppliers.");
                var cheapestProductSuppliers = await FindAllCheapestProductSuppliers();
                Trace.WriteLine(cheapestProductSuppliers);
                if (cheapestProductSuppliers == null || !cheapestProductSuppliers.Any())
                {
                    Trace.WriteLine("No suppliers found.");
                    return NotFound("No suppliers found.");
                }

                return Ok(cheapestProductSuppliers);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        // GET api/Products/5
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
                return Ok(ex.Message);
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

            var companyProductResponse = await _companyProductService.GetCompanyProducts();
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
                        ImageUrl = product.ImageUrl
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
                ImageUrl = product.ImageUrl
            };

            return companyWithProductDto;
        }
    }
}
