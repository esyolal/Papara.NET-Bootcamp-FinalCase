using AutoMapper;
using MediatR;
using Pp.Base.Response;
using Pp.Business.Cqrs;
using Pp.Business.Services;
using Pp.Data.Domain;
using Pp.Data.UnitOfWork;
using Pp.Schema;

namespace Pp.Business.Command
{
    public class ProductCommandHandler :
        IRequestHandler<CreateProductCommand, ApiResponse<ProductResponse>>,
        IRequestHandler<UpdateProductCommand, ApiResponse>,
        IRequestHandler<DeleteProductCommand, ApiResponse>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly ICurrentUser currentUser;

        public ProductCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUser currentUser)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.currentUser = currentUser;
        }

        public async Task<ApiResponse<ProductResponse>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            if (request.Request == null)
            {
                return new ApiResponse<ProductResponse>("Request object cannot be null");
            }

            if (!HasValidAmounts(request.Request))
            {
                return new ApiResponse<ProductResponse>("Invalid price or reward points.");
            }

            var categoryIds = request.Request.CategoryIds.Distinct().ToArray();
            foreach (var categoryId in categoryIds)
            {
                var categoryExists = await unitOfWork.CategoryRepository.ExistsAsync(c => c.Id == categoryId);
                if (!categoryExists)
                {
                    return new ApiResponse<ProductResponse>($"Category with Id {categoryId} does not exist");
                }
            }

            var product = mapper.Map<Product>(request.Request);
            var username = currentUser.GetCurrentUser();
            product.ProductCategories = categoryIds.Select(id => CreateLink(id, username)).ToList();

            try
            {
                await unitOfWork.ProductRepository.InsertAsync(product, username);

                var response = mapper.Map<ProductResponse>(product);
                return new ApiResponse<ProductResponse>(response);
            }
            catch (Exception ex)
            {
                return new ApiResponse<ProductResponse>($"Error creating product: {ex.Message}");
            }
        }




        public async Task<ApiResponse> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            if (!HasValidAmounts(request.Request))
            {
                return new ApiResponse("Invalid price or reward points.");
            }

            var existingProduct = await unitOfWork.ProductRepository.GetById(request.ProductId, nameof(Product.ProductCategories));
            if (existingProduct == null)
            {
                return new ApiResponse("Product not found");
            }

            var categoryIds = request.Request.CategoryIds.ToHashSet();
            foreach (var categoryId in categoryIds)
            {
                if (!await unitOfWork.CategoryRepository.ExistsAsync(category => category.Id == categoryId))
                {
                    return new ApiResponse($"Category with Id {categoryId} does not exist");
                }
            }

            mapper.Map(request.Request, existingProduct);
            var existingLinks = existingProduct.ProductCategories.ToArray();
            foreach (var link in existingLinks.Where(link => !categoryIds.Contains(link.CategoryId)))
            {
                unitOfWork.ProductCategoryRepository.Delete(link);
            }
            foreach (var categoryId in categoryIds.Except(existingLinks.Select(link => link.CategoryId)))
            {
                existingProduct.ProductCategories.Add(CreateLink(categoryId, currentUser.GetCurrentUser()));
            }
            await unitOfWork.Complete();
            return new ApiResponse();
        }

        public async Task<ApiResponse> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var existingProduct = await unitOfWork.ProductRepository.GetById(request.ProductId);
            if (existingProduct == null)
            {
                return new ApiResponse("Product not found");
            }
            var productCategories = await unitOfWork.ProductCategoryRepository.Find(pc => pc.ProductId == request.ProductId);
            foreach (var pc in productCategories)
            {
                unitOfWork.ProductCategoryRepository.Delete(pc);
            }


            unitOfWork.ProductRepository.Delete(existingProduct);
            await unitOfWork.Complete();
            return new ApiResponse();
        }

        private static bool HasValidAmounts(ProductRequest request) =>
            request.Price >= 0 && decimal.Round(request.Price, 2) == request.Price &&
            request.PointsPercentage is >= 0 and <= 100 && request.MaxPoints >= 0;

        private static ProductCategory CreateLink(long categoryId, string username) => new()
        {
            CategoryId = categoryId,
            InsertUser = username,
            InsertDate = DateTime.UtcNow,
            IsActive = true
        };
    }
}
