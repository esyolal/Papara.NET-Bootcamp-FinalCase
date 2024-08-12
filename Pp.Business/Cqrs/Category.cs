using MediatR;
using Pp.Base.Response;
using Pp.Schema;

namespace Pp.Business.Cqrs
{
    public record CreateCategoryCommand(CategoryRequest Request) : IRequest<ApiResponse<CategoryResponse>>;

    public record UpdateCategoryCommand(long CategoryId, CategoryRequest Request) : IRequest<ApiResponse>;

    public record DeleteCategoryCommand(long CategoryId) : IRequest<ApiResponse>;

    public record GetAllCategoriesQuery() : IRequest<ApiResponse<List<CategoryResponse>>>;

    public record GetCategoryByIdQuery(long CategoryId) : IRequest<ApiResponse<CategoryResponse>>;

    public record GetCategoriesByProductQuery(long ProductId) : IRequest<ApiResponse<List<CategoryResponse>>>;
}
