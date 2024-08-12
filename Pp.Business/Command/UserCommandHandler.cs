using AutoMapper;
using MediatR;
using Pp.Base.Response;
using Pp.Business.Cqrs;
using Pp.Business.Services;
using Pp.Business.Token;
using Pp.Data.Domain;
using Pp.Data.UnitOfWork;
using Pp.Schema;

namespace Pp.Business.Command
{
    public class UserCommandHandler :
        IRequestHandler<CreateUserCommand, ApiResponse<UserResponse>>,
        IRequestHandler<UpdateUserCommand, ApiResponse>,
        IRequestHandler<DeleteUserCommand, ApiResponse>,
        IRequestHandler<RegisterUserCommand, ApiResponse<RegisterResponse>>,
        IRequestHandler<LoginUserCommand, ApiResponse<LoginResponse>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly ITokenService tokenService;
        private readonly ICurrentUser currentUser;

        public UserCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ITokenService tokenService, ICurrentUser currentUser)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.tokenService = tokenService;
            this.currentUser = currentUser;
        }

        public async Task<ApiResponse<UserResponse>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            if (request.Request == null)
            {
                return new ApiResponse<UserResponse>("Request object cannot be null");
            }

            var user = mapper.Map<User>(request.Request);
            var username = currentUser.GetCurrentUser();

            if (string.IsNullOrEmpty(user.Password))
            {
                return new ApiResponse<UserResponse>("Password cannot be null or empty");
            }

            try
            {
                user.Password = HashPassword(user.Password);

                await unitOfWork.UserRepository.InsertAsync(user, username);
                await CreateWalletForUser(user.Id);
                await unitOfWork.Complete();

                var response = mapper.Map<UserResponse>(user);
                return new ApiResponse<UserResponse>(response);
            }
            catch (Exception ex)
            {
                return new ApiResponse<UserResponse>($"Error creating user: {ex.Message}");
            }
        }

        public async Task<ApiResponse> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var existingUser = await unitOfWork.UserRepository.GetById(request.UserId);
            if (existingUser == null)
            {
                return new ApiResponse("User not found");
            }

            var updatedUser = mapper.Map(request.Request, existingUser);
            unitOfWork.UserRepository.Update(updatedUser);
            await unitOfWork.Complete();
            return new ApiResponse();
        }

        public async Task<ApiResponse> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var existingUser = await unitOfWork.UserRepository.GetById(request.UserId);
            if (existingUser == null)
            {
                return new ApiResponse("User not found");
            }

            await unitOfWork.UserRepository.Delete(request.UserId);
            await unitOfWork.Complete();
            return new ApiResponse();
        }

        public async Task<ApiResponse<RegisterResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            if (request.Request == null)
            {
                return new ApiResponse<RegisterResponse>("Request object cannot be null");
            }

            var user = mapper.Map<User>(request.Request);
            var username = currentUser.GetCurrentUser();

            if (string.IsNullOrEmpty(user.Password))
            {
                return new ApiResponse<RegisterResponse>("Password cannot be null or empty");
            }

            user.Password = HashPassword(user.Password);
            user.Role = "User";

            await unitOfWork.UserRepository.InsertAsync(user, username);
            await CreateWalletForUser(user.Id);
            await unitOfWork.Complete();

            var response = mapper.Map<RegisterResponse>(user);
            return new ApiResponse<RegisterResponse>(response);
        }

        public async Task<ApiResponse<LoginResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var user = await unitOfWork.UserRepository.FirstOrDefault(u => u.Email == request.Request.Email);
            if (user == null || !VerifyPassword(request.Request.Password, user.Password))
            {
                return new ApiResponse<LoginResponse>("Invalid credentials");
            }

            var token = await tokenService.GenerateTokenAsync(user);
            var response = mapper.Map<LoginResponse>(user);
            response.Token = token;
            return new ApiResponse<LoginResponse>(response);
        }

        private async Task CreateWalletForUser(long userId)
        {
            var wallet = new Wallet
            {
                UserId = userId,
                Points = 0,
                Balance = 0
            };

            await unitOfWork.WalletRepository.InsertAsync(wallet, "System");
        }

        private string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Password cannot be null or empty", nameof(password));
            }
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string inputPassword, string storedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(inputPassword, storedPassword);
        }
    }
}
