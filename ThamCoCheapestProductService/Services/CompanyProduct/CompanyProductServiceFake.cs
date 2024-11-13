
using System.Net;
using System.Text.Json;
using ThamCoCheapestProductService.Dtos;

namespace ThamCoCheapestProductService.Services.CompanyProduct
{
    public class CompanyProductServiceFake : ICompanyProductService
    {
        private readonly IEnumerable<CompanyProductsDto> _companyProducts;
        public CompanyProductServiceFake()
        {
            _companyProducts = new List<CompanyProductsDto>
            {
                new CompanyProductsDto
                {
                    ProductId = 1,
                    CompanyId = 1,
                    Price = 10
                },
                new CompanyProductsDto
                {
                    ProductId = 2,
                    CompanyId = 1,
                    Price = 20
                },

                new CompanyProductsDto
                {
                    ProductId = 3,
                    CompanyId = 1,
                    Price = 10
                },
                new CompanyProductsDto
                {
                    ProductId = 4,
                    CompanyId = 1,
                    Price = 20
                },

                new CompanyProductsDto
                {
                    ProductId = 5,
                    CompanyId = 1,
                    Price = 10
                },
               new CompanyProductsDto
                {
                    ProductId = 1,
                    CompanyId = 2,
                    Price = 20
                },
                new CompanyProductsDto
                {
                    ProductId = 2,
                    CompanyId = 2,
                    Price = 10
                },

                new CompanyProductsDto
                {
                    ProductId = 3,
                    CompanyId = 2,
                    Price = 20
                },
                new CompanyProductsDto
                {
                    ProductId = 4,
                    CompanyId = 2,
                    Price = 10
                },

                new CompanyProductsDto
                {
                    ProductId = 5,
                    CompanyId = 2,
                    Price = 20
                }
            };
        }

        public async Task<HttpResponseMessage> GetCompanyProduct(int companyId, int productId)
        {
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(_companyProducts.FirstOrDefault(p => p.CompanyId == companyId && p.ProductId == productId)))
            };
        }

        public async Task<HttpResponseMessage> GetCompanyProducts()
        {
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(_companyProducts))
            };
        }

    }
}
