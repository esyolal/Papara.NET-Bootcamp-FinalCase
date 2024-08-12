using MediatR;
using Pp.Base.Response;
using Pp.Schema;

namespace Pp.Business.Cqrs
{
    public record CreateProductCommand(ProductRequest Request) : IRequest<ApiResponse<ProductResponse>>;

    public record UpdateProductCommand(long ProductId, ProductRequest Request) : IRequest<ApiResponse>;

    public record DeleteProductCommand(long ProductId) : IRequest<ApiResponse>;

    public record GetAllProductsQuery() : IRequest<ApiResponse<List<ProductResponse>>>;

    public record GetProductByIdQuery(long ProductId) : IRequest<ApiResponse<ProductResponse>>;

    public record GetProductsByCategoryQuery(long CategoryId) : IRequest<ApiResponse<List<ProductResponse>>>;
}
