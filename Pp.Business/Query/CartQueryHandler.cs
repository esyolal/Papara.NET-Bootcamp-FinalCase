using AutoMapper;
using MediatR;
using Pp.Base.Response;
using Pp.Business.Cqrs;
using Pp.Data.UnitOfWork;
using Pp.Schema;

namespace Pp.Business.Query
{
    public class CartQueryHandler :
        IRequestHandler<GetCartByUserIdQuery, ApiResponse<CartResponse>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public CartQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<ApiResponse<CartResponse>> Handle(GetCartByUserIdQuery request, CancellationToken cancellationToken)
        {
            var cart = await unitOfWork.CartRepository.FirstOrDefault(c => c.UserId == request.UserId, "CartItems");
            if (cart == null)
            {
                return new ApiResponse<CartResponse>("Cart not found");
            }

            var response = mapper.Map<CartResponse>(cart);
            return new ApiResponse<CartResponse>(response);
        }
    }
}
