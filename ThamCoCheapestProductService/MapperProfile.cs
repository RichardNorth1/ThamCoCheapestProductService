
using AutoMapper;
using ThamCoCheapestProductService.Dtos;

namespace ThamCoCheapestProductService
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<ProductDto, ProductDto>().ReverseMap();

        }

    }
}
