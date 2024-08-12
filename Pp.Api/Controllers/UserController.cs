using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pp.Base.Response;
using Pp.Business.Cqrs;
using Pp.Business.Token;
using Pp.Schema;

namespace Pp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ITokenService _tokenService;

        public UserController(IMediator mediator, ITokenService tokenService)
        {
            _mediator = mediator;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ApiResponse<RegisterResponse>> Register([FromBody] RegisterRequest request)
        {
            var command = new RegisterUserCommand(request);
            var result = await _mediator.Send(command);
            return result;
        }

        [HttpPost("login")]
        public async Task<ApiResponse<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            var command = new LoginUserCommand(request);
            var result = await _mediator.Send(command);
            return result;
        }

        [HttpPost("create")]
        [Authorize(Roles = "Admin")]
        public async Task<ApiResponse<UserResponse>> CreateUser([FromBody] UserRequest request)
        {
            var command = new CreateUserCommand(request);
            var result = await _mediator.Send(command);
            return result;
        }

        [HttpPut("update/{userId}")]
        public async Task<ApiResponse> UpdateUser(long userId, [FromBody] UserRequest request)
        {
            var command = new UpdateUserCommand(userId, request);
            var result = await _mediator.Send(command);
            return result;
        }

        [HttpDelete("delete/{userId}")]
        public async Task<ApiResponse> DeleteUser(long userId)
        {
            var command = new DeleteUserCommand(userId);
            var result = await _mediator.Send(command);
            return result;
        }
    }
}
