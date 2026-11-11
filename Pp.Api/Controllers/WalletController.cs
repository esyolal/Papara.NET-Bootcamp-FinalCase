using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pp.Base.Response;
using Pp.Business.Cqrs;
using Pp.Business.Services;
using Pp.Schema;

namespace Pp.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class WalletController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly ICurrentUser currentUser;
        private readonly ICreditCardService creditCardService;

        public WalletController(IMediator mediator, ICurrentUser currentUser, ICreditCardService creditCardService)
        {
            this.mediator = mediator;
            this.currentUser = currentUser;
            this.creditCardService = creditCardService;
        }

        [HttpGet("wallet")]
        public async Task<ApiResponse<WalletResponse>> GetWallet(CancellationToken cancellationToken)
        {
            try
            {
                var userId = currentUser.GetCurrentUserId();

                if (userId <= 0)
                {
                    return new ApiResponse<WalletResponse>("Invalid user ID.");
                }

                var query = new GetWalletByUserIdQuery(userId);
                var response = await mediator.Send(query, cancellationToken);

                return response.IsSuccess ? response : new ApiResponse<WalletResponse>(response.Message);
            }
            catch (Exception ex)
            {
                return new ApiResponse<WalletResponse>($"Error retrieving wallet: {ex.Message}");
            }
        }

        [HttpPost("add-funds")]
        public async Task<ApiResponse<WalletResponse>> AddFundsToWallet(WalletRequest request, CancellationToken cancellationToken)
        {
            var userId = currentUser.GetCurrentUserId();
            if (!creditCardService.TryDebit(userId, request.CreditCardNumber, request.ExpiryDate, request.CVV, request.Amount))
            {
                return new ApiResponse<WalletResponse>("Invalid test card, amount, or insufficient card balance.");
            }

            try
            {
                var command = new AddFundsToWalletCommand(userId, request.Amount);
                var response = await mediator.Send(command, cancellationToken);
                if (!response.IsSuccess)
                {
                    creditCardService.Refund(userId, request.CreditCardNumber, request.Amount);
                }
                return response;
            }
            catch
            {
                creditCardService.Refund(userId, request.CreditCardNumber, request.Amount);
                throw;
            }
        }

        [HttpPost("generate-credit-card")]
        public ApiResponse<CreditCardInfo> GenerateCreditCard()
        {
            var creditCardInfo = creditCardService.GenerateCreditCard(currentUser.GetCurrentUserId());
            return new ApiResponse<CreditCardInfo>(creditCardInfo);
        }
    }
}
