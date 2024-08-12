using AutoMapper;
using MediatR;
using Pp.Base.Response;
using Pp.Business.Cqrs;
using Pp.Data.UnitOfWork;
using Pp.Schema;

namespace Pp.Business.Query
{
    public class ProductQueryHandler :
        IRequestHandler<GetAllProductsQuery, ApiResponse<List<ProductResponse>>>,
        IRequestHandler<GetProductByIdQuery, ApiResponse<ProductResponse>>,
        IRequestHandler<GetProductsByCategoryQuery, ApiResponse<List<ProductResponse>>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public ProductQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<ApiResponse<List<ProductResponse>>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
        {
            var products = await unitOfWork.ProductRepository.GetAll();
            var productIds = products.Select(p => p.Id).ToList();

            var productCategories = await unitOfWork.ProductCategoryRepository
                .Find(pc => productIds.Contains(pc.ProductId));
            var categoryIds = productCategories.Select(pc => pc.CategoryId).Distinct().ToList();
            var categories = await unitOfWork.CategoryRepository.Find(c => categoryIds.Contains(c.Id));

            var response = products.Select(product => 
            {
                var productResponse = mapper.Map<ProductResponse>(product);
                var categoryNames = productCategories
                    .Where(pc => pc.ProductId == product.Id)
                    .Select(pc => categories.First(c => c.Id == pc.CategoryId).Name)
                    .ToList();
                productResponse.CategoryNames = categoryNames;
                return productResponse;
            }).ToList();

            return new ApiResponse<List<ProductResponse>>(response);
        }

        public async Task<ApiResponse<ProductResponse>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var product = await unitOfWork.ProductRepository.GetById(request.ProductId);
            if (product == null)
            {
                return new ApiResponse<ProductResponse>("Product not found");
            }

            var productCategories = await unitOfWork.ProductCategoryRepository
                .Find(pc => pc.ProductId == request.ProductId);
            var categoryIds = productCategories.Select(pc => pc.CategoryId).Distinct().ToList();
            var categories = await unitOfWork.CategoryRepository.Find(c => categoryIds.Contains(c.Id));

            var productResponse = mapper.Map<ProductResponse>(product);
            var categoryNames = productCategories
                .Select(pc => categories.First(c => c.Id == pc.CategoryId).Name)
                .ToList();
            productResponse.CategoryNames = categoryNames;

            return new ApiResponse<ProductResponse>(productResponse);
        }

        public async Task<ApiResponse<List<ProductResponse>>> Handle(GetProductsByCategoryQuery request, CancellationToken cancellationToken)
        {
            var productCategories = await unitOfWork.ProductCategoryRepository
                .Find(pc => pc.CategoryId == request.CategoryId);
            var productIds = productCategories.Select(pc => pc.ProductId).Distinct().ToList();
            var products = await unitOfWork.ProductRepository.Find(p => productIds.Contains(p.Id));
            var categoryIds = productCategories.Select(pc => pc.CategoryId).Distinct().ToList();
            var categories = await unitOfWork.CategoryRepository.Find(c => categoryIds.Contains(c.Id));

            var response = products.Select(product => 
            {
                var productResponse = mapper.Map<ProductResponse>(product);
                var categoryNames = productCategories
                    .Where(pc => pc.ProductId == product.Id)
                    .Select(pc => categories.First(c => c.Id == pc.CategoryId).Name)
                    .ToList();
                productResponse.CategoryNames = categoryNames;
                return productResponse;
            }).ToList();

            return new ApiResponse<List<ProductResponse>>(response);
        }
    }
}
