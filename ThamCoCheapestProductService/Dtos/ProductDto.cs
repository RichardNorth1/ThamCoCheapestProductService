

namespace ThamCoCheapestProductService.Dtos
{
    public class ProductDto
    {
        public ProductDto()
        {
            
        }

        public ProductDto(int productId, 
            string name,
            string brand, 
            string description, 
            double price, 
            string imageUrl)
        {
            ProductId = productId;
            Name = name;
            Brand = brand;
            Description = description;
            Price = price;
            ImageUrl = imageUrl;
        }

        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public string ImageUrl { get; set; }
    }
}
