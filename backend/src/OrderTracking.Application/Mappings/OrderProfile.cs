using AutoMapper;
using OrderTracking.Application.DTOs;
using OrderTracking.Domain.Entities;

namespace OrderTracking.Application.Mappings
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        }
    }
}
