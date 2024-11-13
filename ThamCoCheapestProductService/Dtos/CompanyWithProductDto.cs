namespace ThamCoCheapestProductService.Dtos
{
    public class CompanyWithProductDto
    {
        public int ProductId { get; set; }
        public int CompanyId { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public string ImageUrl { get; set; }
    }
}
