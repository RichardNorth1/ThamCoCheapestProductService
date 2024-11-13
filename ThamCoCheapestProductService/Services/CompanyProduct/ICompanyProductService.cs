namespace ThamCoCheapestProductService.Services
{
    public interface ICompanyProductService
    {
        Task<HttpResponseMessage> GetCompanyProduct(int companyId, int productId);
        Task<HttpResponseMessage> GetCompanyProducts();
    }
}
