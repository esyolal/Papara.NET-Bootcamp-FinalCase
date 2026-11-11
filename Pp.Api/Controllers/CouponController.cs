using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pp.Business.Services;
using Pp.Data.Context;
using Pp.Data.Domain;

namespace Pp.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
public class CouponController(AppDbContext dbContext, ICurrentUser currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CouponResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var coupons = await dbContext.Coupons.AsNoTracking().OrderBy(c => c.Code)
            .Select(c => new CouponResponse(c.Code, c.DiscountAmount, c.ValidityStartDate, c.ValidityEndDate,
                c.IsActive, c.UsedByUserId != null))
            .ToListAsync(cancellationToken);
        return Ok(coupons);
    }

    [HttpDelete("{code}")]
    public async Task<IActionResult> Deactivate(string code, CancellationToken cancellationToken)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        var updated = await dbContext.Coupons.Where(c => c.Code == normalizedCode)
            .ExecuteUpdateAsync(setters => setters.SetProperty(c => c.IsActive, false), cancellationToken);
        return updated == 0 ? NotFound() : NoContent();
    }

    [HttpPost]
    public async Task<ActionResult<CouponResponse>> Create(CouponRequest request, CancellationToken cancellationToken)
    {
        var code = request.Code?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(code) || code.Length > 64 || request.DiscountAmount <= 0 ||
            request.DiscountAmount > 9999999999999999.99m ||
            decimal.Round(request.DiscountAmount, 2) != request.DiscountAmount ||
            request.StartsAt >= request.EndsAt || request.EndsAt <= DateTimeOffset.UtcNow)
        {
            return BadRequest("Provide a code, a positive discount and a valid date range.");
        }

        var coupon = new Coupon
        {
            Code = code,
            DiscountAmount = request.DiscountAmount,
            ValidityStartDate = request.StartsAt.UtcDateTime,
            ValidityEndDate = request.EndsAt.UtcDateTime,
            IsActive = true,
            InsertDate = DateTime.UtcNow,
            InsertUser = currentUser.GetCurrentUser()
        };
        dbContext.Coupons.Add(coupon);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is Microsoft.Data.SqlClient.SqlException { Number: 2601 or 2627 })
        {
            return Conflict("A coupon with this code already exists.");
        }

        return StatusCode(StatusCodes.Status201Created, new CouponResponse(
            code, coupon.DiscountAmount, coupon.ValidityStartDate, coupon.ValidityEndDate, true, false));
    }

    public record CouponRequest(string Code, decimal DiscountAmount, DateTimeOffset StartsAt, DateTimeOffset EndsAt);
    public record CouponResponse(string Code, decimal DiscountAmount, DateTime StartsAt, DateTime EndsAt,
        bool IsActive, bool IsUsed);
}
