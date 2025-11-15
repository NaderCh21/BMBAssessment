using AutoMapper;
using ProductService.Domain.Entities;
using ProductService.DTOs;

namespace ProductService.Mapping
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<Product, ProductDto>();

            CreateMap<CreateProductDto, Product>();

            CreateMap<UpdateProductDto, Product>();
        }
    }
}
