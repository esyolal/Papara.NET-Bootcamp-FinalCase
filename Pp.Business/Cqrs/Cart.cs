using MediatR;
using Pp.Base.Response;
using Pp.Data.Domain;
using Pp.Schema;

namespace Pp.Business.Cqrs
{
    public record AddProductToCartCommand(long ProductId, int Quantity) : IRequest<ApiResponse>;
    public record RemoveProductFromCartCommand(long ProductId, int Quantity) : IRequest<ApiResponse>;
    public record GetCartByUserIdQuery(long UserId) : IRequest<ApiResponse<CartResponse>>;


}
