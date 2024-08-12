using MediatR;
using Pp.Base.Response;
using Pp.Business.Cqrs;
using Pp.Business.Services;
using Pp.Data.UnitOfWork;

namespace Pp.Business.Command;

public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, ApiResponse>
{
    private readonly IUnitOfWork unitOfWork;
    private readonly ICurrentUser currentUser;

    public ProcessPaymentCommandHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    {
        this.unitOfWork = unitOfWork;
        this.currentUser = currentUser;
    }

    public async Task<ApiResponse> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.GetCurrentUserId();
        var cart = await unitOfWork.CartRepository.FirstOrDefault(c => c.Id == request.CartId && c.UserId == userId);

        if (cart == null)
        {
            return new ApiResponse("Cart not found");
        }

        if (cart.TotalCost <= 0)
        {
            return new ApiResponse("Cart is empty");
        }

        var wallet = await unitOfWork.WalletRepository.FirstOrDefault(w => w.UserId == userId);

        if (wallet == null)
        {
            return new ApiResponse("Wallet not found");
        }

        if (wallet.Balance < cart.TotalCost)
        {
            return new ApiResponse("Insufficient balance");
        }

        unitOfWork.BeginTransaction();
        try
        {
            wallet.Balance -= cart.TotalCost;
            unitOfWork.WalletRepository.Update(wallet);

            unitOfWork.CartItemRepository.DeleteRange(cart.Items);
            unitOfWork.CartRepository.Delete(cart);

            await unitOfWork.Complete();

            unitOfWork.CommitTransaction();
        }
        catch
        {
    
            unitOfWork.RollbackTransaction();
            throw;
        }

        return new ApiResponse("Payment successful");
    }
}
