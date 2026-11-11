using AutoMapper;
using Pp.Data.Domain;
using Pp.Schema;

namespace Pp.Business.Mapper
{
    public class MapperConfig : Profile
    {
        public MapperConfig()
        {
            CreateMap<UserRequest, User>();

            CreateMap<User, UserResponse>();

            CreateMap<User, RegisterResponse>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id.ToString()));
            CreateMap<User, LoginResponse>()
           .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id.ToString()))
           .ForMember(dest => dest.Name, opt => opt.MapFrom(src => $"{src.Name} {src.Surname}"))
           .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));

            CreateMap<Category, CategoryResponse>();
            CreateMap<CategoryRequest, Category>();

            CreateMap<Product, ProductResponse>();
            CreateMap<ProductRequest, Product>();

            CreateMap<Cart, CartResponse>()
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.Items.Sum(item => item.Price)));
            CreateMap<CartRequest, Cart>();

            CreateMap<CartItem, CartItemResponse>();
            CreateMap<CartItemRequest, CartItem>();

            CreateMap<Wallet, WalletResponse>();
            CreateMap<WalletRequest, Wallet>();

            CreateMap<RegisterRequest, User>();
        }
    }
}
