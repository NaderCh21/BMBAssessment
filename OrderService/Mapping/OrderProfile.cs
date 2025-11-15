using AutoMapper;
using OrderService.Domain.DTO;
using OrderService.Domain.Entities;

namespace OrderService.Mapping
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<CreateOrderDto, Order>();
            CreateMap<Order, OrderDto>();
            CreateMap<UpdateOrderDto, Order>();
        }
    }
}
