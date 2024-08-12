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
    public class CategoryCommandHandler :
        IRequestHandler<CreateCategoryCommand, ApiResponse<CategoryResponse>>,
        IRequestHandler<UpdateCategoryCommand, ApiResponse>,
        IRequestHandler<DeleteCategoryCommand, ApiResponse>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly ICurrentUser currentUser;

        public CategoryCommandHandler(IUnitOfWork unitOfWork, IMapper mapper,ICurrentUser currentUser)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.currentUser = currentUser;
        }

        public async Task<ApiResponse<CategoryResponse>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            if (request.Request == null)
            {
                return new ApiResponse<CategoryResponse>("Request object cannot be null");
            }

            var category = mapper.Map<Category>(request.Request);
            var name = currentUser.GetCurrentUser();
            try
            {
                await unitOfWork.CategoryRepository.InsertAsync(category, name);
                await unitOfWork.Complete();
                var response = mapper.Map<CategoryResponse>(category);
                return new ApiResponse<CategoryResponse>(response);
            }
            catch (Exception ex)
            {
                return new ApiResponse<CategoryResponse>($"Error creating category: {ex.Message}");
            }
        }

        public async Task<ApiResponse> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            var existingCategory = await unitOfWork.CategoryRepository.GetById(request.CategoryId);
            if (existingCategory == null)
            {
                return new ApiResponse("Category not found");
            }

            var updatedCategory = mapper.Map(request.Request, existingCategory);
            unitOfWork.CategoryRepository.Update(updatedCategory);
            await unitOfWork.Complete();
            return new ApiResponse();
        }

        public async Task<ApiResponse> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            var existingCategory = await unitOfWork.CategoryRepository.GetById(request.CategoryId);
            if (existingCategory == null)
            {
                return new ApiResponse("Category not found");
            }

            var categoryProducts = await unitOfWork.ProductCategoryRepository.Find(pc => pc.CategoryId == request.CategoryId);
            foreach (var pc in categoryProducts)
            {
                await unitOfWork.ProductCategoryRepository.Delete(pc.Id);
            }

            await unitOfWork.CategoryRepository.Delete(request.CategoryId);
            await unitOfWork.Complete();
            return new ApiResponse();
        }
    }
}
