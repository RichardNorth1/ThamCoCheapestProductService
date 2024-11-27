using ThamCoCheapestProductService.Dtos;

namespace ThamCoCheapestProductService.Services.Token
{
    public interface ITokenService
    {
        public Task<TokenDto> GetToken();
    }
}
