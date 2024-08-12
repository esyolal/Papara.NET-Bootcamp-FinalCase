using AutoMapper;
using MediatR;
using Pp.Base.Response;
using Pp.Business.Cqrs;
using Pp.Data.UnitOfWork;
using Pp.Schema;

namespace Pp.Business.Query
{
    public class CategoryQueryHandler :
        IRequestHandler<GetAllCategoriesQuery, ApiResponse<List<CategoryResponse>>>,
        IRequestHandler<GetCategoryByIdQuery, ApiResponse<CategoryResponse>>,
        IRequestHandler<GetCategoriesByProductQuery, ApiResponse<List<CategoryResponse>>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public CategoryQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<ApiResponse<List<CategoryResponse>>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
        {
            var categories = await unitOfWork.CategoryRepository.GetAll();
            var response = mapper.Map<List<CategoryResponse>>(categories);
            return new ApiResponse<List<CategoryResponse>>(response);
        }

        public async Task<ApiResponse<CategoryResponse>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
        {
            var category = await unitOfWork.CategoryRepository.GetById(request.CategoryId);
            var response = mapper.Map<CategoryResponse>(category);
            return new ApiResponse<CategoryResponse>(response);
        }

        public async Task<ApiResponse<List<CategoryResponse>>> Handle(GetCategoriesByProductQuery request, CancellationToken cancellationToken)
        {
            var productCategories = await unitOfWork.ProductCategoryRepository.Find(pc => pc.ProductId == request.ProductId);
            var categoryIds = productCategories.Select(pc => pc.CategoryId).Distinct();
            var categories = await unitOfWork.CategoryRepository.Find(c => categoryIds.Contains(c.Id));
            var response = mapper.Map<List<CategoryResponse>>(categories);
            return new ApiResponse<List<CategoryResponse>>(response);
        }
    }
}
