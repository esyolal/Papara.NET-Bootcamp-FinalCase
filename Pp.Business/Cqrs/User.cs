using MediatR;
using Pp.Base.Response;
using Pp.Schema;

namespace Pp.Business.Cqrs;

public record CreateUserCommand(UserRequest Request) : IRequest<ApiResponse<UserResponse>>;

public record UpdateUserCommand(long UserId, UserRequest Request) : IRequest<ApiResponse>;

public record DeleteUserCommand(long UserId) : IRequest<ApiResponse>;
public record LoginUserCommand(LoginRequest Request) : IRequest<ApiResponse<LoginResponse>>;
public record RegisterUserCommand(RegisterRequest Request) : IRequest<ApiResponse<RegisterResponse>>;

public record GetAllUsersQuery() : IRequest<ApiResponse<List<UserResponse>>>;

public record GetUserByIdQuery(long UserId) : IRequest<ApiResponse<UserResponse>>;

public record GetUserByParametersQuery(string Email, string Name) : IRequest<ApiResponse<List<UserResponse>>>;