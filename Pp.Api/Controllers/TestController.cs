using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pp.Base.Response;
using Pp.Business.Services;

namespace Pp.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly ICurrentUser currentUser;

        public TestController(IMediator mediator, ICurrentUser currentUser)
        {
            this.mediator = mediator;
            this.currentUser = currentUser;
        }

        [HttpGet("current-user-id")]
        public IActionResult GetCurrentUserId()
        {
            try
            {
                var userId = currentUser.GetCurrentUserId();
                return Ok(new ApiResponse<long>(userId));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<long>($"Error retrieving user ID: {ex.Message}"));
            }
        }
    }
}
