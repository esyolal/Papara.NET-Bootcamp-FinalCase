using MediatR;
using System.Text.Json;
using Pp.Base.Response;
using Pp.Business.Cqrs;
using Pp.Business.Services;
using Pp.Data.Domain;
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
        var cart = await unitOfWork.CartRepository.FirstOrDefault(
            c => c.Id == request.CartId && c.UserId == userId, nameof(Cart.Items));

        if (cart == null)
        {
            return new ApiResponse("Cart not found");
        }

        var subtotal = cart.Items.Sum(item => item.Price);
        if (subtotal <= 0)
        {
            return new ApiResponse("Cart is empty");
        }

        var wallet = await unitOfWork.WalletRepository.FirstOrDefault(w => w.UserId == userId);

        if (wallet == null)
        {
            return new ApiResponse("Wallet not found");
        }

        Coupon? coupon = null;
        var total = subtotal;
        if (!string.IsNullOrWhiteSpace(request.CouponCode))
        {
            var code = request.CouponCode.Trim().ToUpperInvariant();
            var now = DateTime.UtcNow;
            coupon = await unitOfWork.CouponRepository.FirstOrDefault(c =>
                c.Code == code && c.IsActive && c.UsedByUserId == null &&
                c.ValidityStartDate <= now && c.ValidityEndDate > now && c.DiscountAmount > 0);
            if (coupon == null)
            {
                return new ApiResponse("Coupon is invalid, expired, or already used");
            }

            total -= Math.Min(total, coupon.DiscountAmount);
        }

        var pointsUsed = request.UsePoints ? Math.Min(total, Math.Max(0m, wallet.Points)) : 0m;
        total -= pointsUsed;
        if (wallet.Balance < total)
        {
            return new ApiResponse("Insufficient balance");
        }

        var productIds = cart.Items.Select(item => item.ProductId).Distinct().ToArray();
        var products = await unitOfWork.ProductRepository.Find(product => productIds.Contains(product.Id));
        var rewards = products.ToDictionary(product => product.Id);
        var pointsEarned = decimal.Round(cart.Items.Sum(item =>
            rewards.TryGetValue(item.ProductId, out var product)
                ? Math.Min(Math.Max(0m, product.MaxPoints),
                    item.Price * total / subtotal * Math.Clamp(product.PointsPercentage, 0m, 100m) / 100m)
                : 0m), 2);

        unitOfWork.BeginTransaction();
        try
        {
            wallet.Balance -= total;
            wallet.Points += pointsEarned - pointsUsed;
            unitOfWork.WalletRepository.Update(wallet);
            if (coupon != null)
            {
                coupon.UsedByUserId = userId;
            }
            unitOfWork.OrderRepository.Add(new OrderRecord
            {
                UserId = userId,
                Subtotal = subtotal,
                CouponCode = coupon?.Code,
                CouponDiscount = subtotal - pointsUsed - total,
                PointsUsed = pointsUsed,
                AmountPaid = total,
                ItemsJson = JsonSerializer.Serialize(cart.Items.Select(item =>
                    new Pp.Schema.PurchasedItem(item.ProductId, item.Quantity, item.Price))),
                InsertUser = currentUser.GetCurrentUser(),
                InsertDate = DateTime.UtcNow,
                IsActive = true
            });

            // The database cascades the cart deletion to its items.
            unitOfWork.CartRepository.Delete(cart);

            await unitOfWork.Complete();

            unitOfWork.CommitTransaction();
        }
        catch
        {
    
            unitOfWork.RollbackTransaction();
            throw;
        }

        return new ApiResponse(true, "Payment successful");
    }
}
