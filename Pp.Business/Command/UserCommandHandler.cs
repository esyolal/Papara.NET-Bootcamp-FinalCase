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

            if (!IsValidPassword(user.Password))
            {
                return new ApiResponse<UserResponse>("Password must contain between 8 characters and 72 UTF-8 bytes.");
            }

            try
            {
                user.Password = HashPassword(user.Password);

                await InsertUserWithWallet(user, username);

                var response = mapper.Map<UserResponse>(user);
                return new ApiResponse<UserResponse>(response);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
            {
                return new ApiResponse<UserResponse>("Unable to create user.");
            }
        }

        public async Task<ApiResponse> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var existingUser = await unitOfWork.UserRepository.GetById(request.UserId);
            if (existingUser == null)
            {
                return new ApiResponse("User not found");
            }

            var password = request.Request.Password;
            if (password != null && !IsValidPassword(password))
            {
                return new ApiResponse("Password must contain between 8 characters and 72 UTF-8 bytes.");
            }

            existingUser.Name = request.Request.Name;
            existingUser.Surname = request.Request.Surname;
            existingUser.Email = request.Request.Email;
            existingUser.Phone = request.Request.Phone;
            if (password != null)
            {
                existingUser.Password = HashPassword(password);
            }
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

            if (!IsValidPassword(user.Password))
            {
                return new ApiResponse<RegisterResponse>("Password must contain between 8 characters and 72 UTF-8 bytes.");
            }

            user.Password = HashPassword(user.Password);
            user.Role = "User";

            try
            {
                await InsertUserWithWallet(user, username);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
            {
                return new ApiResponse<RegisterResponse>("Unable to register user.");
            }

            var response = mapper.Map<RegisterResponse>(user);
            return new ApiResponse<RegisterResponse>(response);
        }

        public async Task<ApiResponse<LoginResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var user = await unitOfWork.UserRepository.FirstOrDefault(u => u.Email == request.Request.Email);
            if (user == null || !user.IsActive || !VerifyPassword(request.Request.Password, user.Password))
            {
                return new ApiResponse<LoginResponse>("Invalid credentials");
            }

            var token = await tokenService.GenerateTokenAsync(user);
            var response = mapper.Map<LoginResponse>(user);
            response.Token = token;
            return new ApiResponse<LoginResponse>(response);
        }

        private Task InsertUserWithWallet(User user, string username)
        {
            user.Wallet = new Wallet
            {
                IsActive = true,
                InsertDate = DateTime.UtcNow,
                InsertUser = "System"
            };

            return unitOfWork.UserRepository.InsertAsync(user, username);
        }

        private static bool IsValidPassword(string password) =>
            !string.IsNullOrWhiteSpace(password) && password.Length >= 8 &&
            System.Text.Encoding.UTF8.GetByteCount(password) <= 72;

        private static string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);

        private static bool VerifyPassword(string inputPassword, string storedPassword)
        {
            if (string.IsNullOrEmpty(inputPassword) || !storedPassword.StartsWith("$2", StringComparison.Ordinal))
            {
                return false;
            }

            try
            {
                return BCrypt.Net.BCrypt.Verify(inputPassword, storedPassword);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                return false;
            }
        }
    }
}
