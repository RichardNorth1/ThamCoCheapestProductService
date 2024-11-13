using System.Net;
using System.Text.Json;
using ThamCoCheapestProductService.Dtos;

namespace ThamCoCheapestProductService.Services.Product
{
    public class ProductServiceFake: IProductService
    {
        private readonly IEnumerable<ProductDto> _products;
        public ProductServiceFake()
        {
            _products = new List<ProductDto>
            {
                new ProductDto
                {
                    ProductId = 1,
                    Name = "Product 1",
                    Brand = "Brand 1",
                    Description = "Description 1",
                    ImageUrl = "imageUrl"
                },
                new ProductDto
                {
                    ProductId = 2,
                    Name = "Product 2",
                    Brand = "Brand 2",
                    Description = "Description 2",
                    ImageUrl = "imageUrl"
                },

                new ProductDto
                {
                    ProductId = 3,
                    Name = "Product 3",
                    Brand = "Brand 3",
                    Description = "Description 3",
                    ImageUrl = "imageUrl"
                },
                new ProductDto
                {
                    ProductId = 4,
                    Name = "Product 4",
                    Brand = "Brand 4",
                    Description = "Description 4",
                    ImageUrl = "imageUrl"
                },

                new ProductDto
                {
                    ProductId = 5,
                    Name = "Product 5",
                    Brand = "Brand 5",
                    Description = "Description 5",
                    ImageUrl = "imageUrl"
                }
            };
        }

        public async Task<HttpResponseMessage> GetProduct(int productId)
        {
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(_products.FirstOrDefault(p => p.ProductId == productId)))
            };
        }

        public async Task<HttpResponseMessage> GetProducts()
        {
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(_products))
            };
        }
    }
}
