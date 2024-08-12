using AutoMapper;
using MediatR;
using Pp.Base.Response;
using Pp.Business.Cqrs;
using Pp.Data.UnitOfWork;
using Pp.Schema;

namespace Pp.Business.Query;

public class UserQueryHandler :
    IRequestHandler<GetAllUsersQuery, ApiResponse<List<UserResponse>>>,
    IRequestHandler<GetUserByIdQuery, ApiResponse<UserResponse>>,
    IRequestHandler<GetUserByParametersQuery, ApiResponse<List<UserResponse>>>
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IMapper mapper;

    public UserQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        this.unitOfWork = unitOfWork;
        this.mapper = mapper;
    }

    public async Task<ApiResponse<List<UserResponse>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await unitOfWork.UserRepository.GetAll();
        var response = mapper.Map<List<UserResponse>>(users);
        return new ApiResponse<List<UserResponse>>(response);
    }

    public async Task<ApiResponse<UserResponse>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await unitOfWork.UserRepository.GetById(request.UserId);
        var response = mapper.Map<UserResponse>(user);
        return new ApiResponse<UserResponse>(response);
    }

    public async Task<ApiResponse<List<UserResponse>>> Handle(GetUserByParametersQuery request, CancellationToken cancellationToken)
    {
        var users = await unitOfWork.UserRepository.Find(u => u.Email == request.Email && u.Name.Contains(request.Name));
        var response = mapper.Map<List<UserResponse>>(users);
        return new ApiResponse<List<UserResponse>>(response);
    }
}
