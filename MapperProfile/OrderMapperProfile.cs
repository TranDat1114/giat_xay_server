using AutoMapper; // Import the necessary namespace

namespace giat_xay_server;

public class OrderMapperProfile : Profile
{
    public OrderMapperProfile()
    {
        CreateMap<Order, OrderRequest>().ReverseMap().IgnoreAllPropertiesWithAnInaccessibleSetter();
    }
}
