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

            foreach (var categoryId in request.Request.CategoryIds)
            {
                var categoryExists = await unitOfWork.CategoryRepository.ExistsAsync(c => c.Id == categoryId);
                if (!categoryExists)
                {
                    return new ApiResponse<ProductResponse>($"Category with Id {categoryId} does not exist");
                }
            }

            var product = mapper.Map<Product>(request.Request);
            var username = currentUser.GetCurrentUser();

            try
            {
                await unitOfWork.ProductRepository.InsertAsync(product, username);
                await unitOfWork.Complete();

                foreach (var categoryId in request.Request.CategoryIds)
                {
                    var productCategory = new ProductCategory
                    {
                        ProductId = product.Id,
                        CategoryId = categoryId
                    };

                    var categoryExists = await unitOfWork.CategoryRepository.ExistsAsync(c => c.Id == categoryId);
                    if (!categoryExists)
                    {
                        throw new Exception($"Category with Id {categoryId} does not exist");
                    }

                    await unitOfWork.ProductCategoryRepository.InsertAsync(productCategory, username);
                }

                await unitOfWork.Complete();

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
            var existingProduct = await unitOfWork.ProductRepository.GetById(request.ProductId);
            if (existingProduct == null)
            {
                return new ApiResponse("Product not found");
            }

            var updatedProduct = mapper.Map(request.Request, existingProduct);
            unitOfWork.ProductRepository.Update(updatedProduct);


            var existingProductCategories = await unitOfWork.ProductCategoryRepository.Find(pc => pc.ProductId == request.ProductId);
            foreach (var pc in existingProductCategories)
            {
                await unitOfWork.ProductCategoryRepository.Delete(pc.Id);
            }


            foreach (var categoryId in request.Request.CategoryIds)
            {
                var productCategory = new ProductCategory
                {
                    ProductId = updatedProduct.Id,
                    CategoryId = categoryId
                };
                await unitOfWork.ProductCategoryRepository.InsertAsync(productCategory, currentUser.GetCurrentUser());
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
                await unitOfWork.ProductCategoryRepository.Delete(pc.Id);
            }


            await unitOfWork.ProductRepository.Delete(request.ProductId);
            await unitOfWork.Complete();
            return new ApiResponse();
        }

    }
}
