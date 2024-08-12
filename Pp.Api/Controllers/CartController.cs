using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Pp.Base.Response;
using Pp.Business.Cqrs;
using System.Threading.Tasks;
using Pp.Business.Services;
using Pp.Schema;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly IMediator mediator;
    private readonly ICurrentUser currentUser;

    public CartController(IMediator mediator, ICurrentUser currentUser)
    {
        this.currentUser = currentUser;
        this.mediator = mediator;
    }

    [HttpPost("add")]
    public async Task<ApiResponse> AddProduct([FromBody] AddProductToCartCommand command)
    {
        var response = await mediator.Send(command);
        return response;
    }

    [HttpPost("remove")]
    public async Task<ApiResponse> RemoveProduct([FromBody] RemoveProductFromCartCommand command)
    {
        var response = await mediator.Send(command);
        return response;
    }

    [HttpGet]
    public async Task<ApiResponse<CartResponse>> GetCart()
    {
        var userId = currentUser.GetCurrentUserId();
        var query = new GetCartByUserIdQuery(userId);
        var response = await mediator.Send(query);
        return response;
    }

    [HttpPost("checkout")]
    public async Task<ApiResponse> Checkout([FromBody] ProcessPaymentCommand command)
    {
        var response = await mediator.Send(command);
        return response;
    }
}
