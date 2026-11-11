using Pp.Base.Response;
using Pp.Business.Cqrs;
using Pp.Data.Domain;
using Pp.Data.UnitOfWork;
using AutoMapper;
using MediatR;
using Pp.Business.Services;

namespace Pp.Business.Command;
public class CartCommandHandler :
    IRequestHandler<AddProductToCartCommand, ApiResponse>,
    IRequestHandler<RemoveProductFromCartCommand, ApiResponse>
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IMapper mapper;
    private readonly ICurrentUser currentUser;

    public CartCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUser currentUser)
    {
        this.unitOfWork = unitOfWork;
        this.mapper = mapper;
        this.currentUser = currentUser;
    }

    public async Task<ApiResponse> Handle(AddProductToCartCommand request, CancellationToken cancellationToken)
    {
        if (request.Quantity <= 0)
        {
            return new ApiResponse("Quantity must be positive");
        }

        var product = await unitOfWork.ProductRepository.FirstOrDefault(p => p.Id == request.ProductId && p.IsActive);
        if (product == null)
        {
            return new ApiResponse("Product not found");
        }

        var userId = currentUser.GetCurrentUserId();
        var cart = await unitOfWork.CartRepository.FirstOrDefault(c => c.UserId == userId, nameof(Cart.Items));
        var lineAmount = product.Price * request.Quantity;

        if (cart == null)
        {
            cart = new Cart { UserId = userId, TotalCost = lineAmount };
            cart.Items.Add(CreateItem(request, lineAmount));
            await unitOfWork.CartRepository.InsertAsync(cart, currentUser.GetCurrentUser());
            return new ApiResponse();
        }

        var cartItem = cart.Items.FirstOrDefault(item => item.ProductId == request.ProductId);
        if (cartItem != null)
        {
            if (request.Quantity > int.MaxValue - cartItem.Quantity)
            {
                return new ApiResponse("Quantity is too large");
            }
            cartItem.Quantity += request.Quantity;
            cartItem.Price += lineAmount;
        }
        else
        {
            cart.Items.Add(CreateItem(request, lineAmount));
        }

        cart.TotalCost = cart.Items.Sum(item => item.Price);
        await unitOfWork.Complete();
        return new ApiResponse();
    }

    public async Task<ApiResponse> Handle(RemoveProductFromCartCommand request, CancellationToken cancellationToken)
    {
        if (request.Quantity <= 0)
        {
            return new ApiResponse("Quantity must be positive");
        }

        var userId = currentUser.GetCurrentUserId();
        var cart = await unitOfWork.CartRepository.FirstOrDefault(c => c.UserId == userId, nameof(Cart.Items));

        if (cart == null)
        {
            return new ApiResponse("Cart not found");
        }

        var cartItem = cart.Items.FirstOrDefault(item => item.ProductId == request.ProductId);
        if (cartItem == null)
        {
            return new ApiResponse("Product not found in cart");
        }

        if (request.Quantity > cartItem.Quantity)
        {
            return new ApiResponse("Quantity exceeds the amount in the cart");
        }

        var priceToRemove = request.Quantity == cartItem.Quantity
            ? cartItem.Price
            : decimal.Round(cartItem.Price * request.Quantity / cartItem.Quantity, 2);

        cartItem.Quantity -= request.Quantity;
        if (cartItem.Quantity <= 0)
        {
            unitOfWork.CartItemRepository.Delete(cartItem);
            cart.Items.Remove(cartItem);
        }
        else
        {
            cartItem.Price -= priceToRemove; 
        }


        cart.TotalCost = cart.Items.Sum(item => item.Price);

        await unitOfWork.Complete();
        return new ApiResponse();
    }

    private CartItem CreateItem(AddProductToCartCommand request, decimal lineAmount) => new()
    {
        ProductId = request.ProductId,
        Quantity = request.Quantity,
        Price = lineAmount,
        IsActive = true,
        InsertDate = DateTime.UtcNow,
        InsertUser = currentUser.GetCurrentUser()
    };
}
