using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pp.Base.Response;
using Pp.Business.Cqrs;
using Pp.Business.Services;
using Pp.Schema;

namespace Pp.WebApi.Controllers
{
    [ApiController]
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
            try
            {
                var userId = currentUser.GetCurrentUserId();

                if (!creditCardService.ValidateCreditCard(request.CreditCardNumber, request.ExpiryDate, request.CVV))
                {
                    return new ApiResponse<WalletResponse>("Invalid credit card details.");
                }

                if (!creditCardService.ValidateTransaction(request.Amount))
                {
                    return new ApiResponse<WalletResponse>("Insufficient card balance.");
                }

                var command = new AddFundsToWalletCommand(userId, request.Amount);
                var response = await mediator.Send(command, cancellationToken);

                return response;
            }
            catch (Exception ex)
            {
                return new ApiResponse<WalletResponse>($"Error adding funds: {ex.Message}");
            }
        }

        [HttpPost("generate-credit-card")]
        public ApiResponse<CreditCardInfo> GenerateCreditCard()
        {
            var creditCardInfo = creditCardService.GenerateCreditCard();
            return new ApiResponse<CreditCardInfo>(creditCardInfo);
        }
    }
}
