using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pp.Business.Services;
using Pp.Data.Context;
using Pp.Schema;

namespace Pp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class OrderController(AppDbContext dbContext, ICurrentUser currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PurchaseResponse>>> GetOrders(
        [FromQuery] long? beforeId, CancellationToken cancellationToken)
    {
        var userId = currentUser.GetCurrentUserId();
        var orders = await dbContext.Orders.AsNoTracking()
            .Where(order => order.UserId == userId && (beforeId == null || order.Id < beforeId))
            .OrderByDescending(order => order.Id)
            .Take(50)
            .ToListAsync(cancellationToken);

        return Ok(orders.Select(order => new PurchaseResponse(order.Id, order.InsertDate, order.Subtotal,
            order.CouponCode, order.CouponDiscount, order.PointsUsed, order.AmountPaid,
            JsonSerializer.Deserialize<PurchasedItem[]>(order.ItemsJson) ?? [])));
    }

    public record PurchaseResponse(long Id, DateTime CreatedAt, decimal Subtotal, string? CouponCode,
        decimal CouponDiscount, decimal PointsUsed, decimal AmountPaid, IReadOnlyList<PurchasedItem> Items);
}
