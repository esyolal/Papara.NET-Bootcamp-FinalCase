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
        var userId = currentUser.GetCurrentUserId();
        var cart = await unitOfWork.CartRepository.FirstOrDefault(c => c.UserId == userId);

        if (cart == null)
        {
            cart = new Cart { UserId = userId };
            await unitOfWork.CartRepository.InsertAsync(cart, currentUser.GetCurrentUser());
        }

        var product = await unitOfWork.ProductRepository.FirstOrDefault(p => p.Id == request.ProductId);
        if (product == null)
        {
            return new ApiResponse("Product not found");
        }

        var cartItem = await unitOfWork.CartItemRepository.FirstOrDefault(ci => ci.CartId == cart.Id && ci.ProductId == request.ProductId);
        if (cartItem != null)
        {
           
            var previousQuantity = cartItem.Quantity;
            cartItem.Quantity += request.Quantity;
            cartItem.Price += request.Quantity * product.Price; 

            cart.TotalCost += (request.Quantity * product.Price); 

            unitOfWork.CartItemRepository.Update(cartItem);
        }
        else
        {
       
            cartItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                Price = request.Quantity * product.Price 
            };
            await unitOfWork.CartItemRepository.InsertAsync(cartItem, currentUser.GetCurrentUser());

          
            cart.TotalCost += cartItem.Price; 
        }

        unitOfWork.CartRepository.Update(cart); 
        await unitOfWork.Complete();
        return new ApiResponse();
    }

    public async Task<ApiResponse> Handle(RemoveProductFromCartCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.GetCurrentUserId();
        var cart = await unitOfWork.CartRepository.FirstOrDefault(c => c.UserId == userId);

        if (cart == null)
        {
            return new ApiResponse("Cart not found");
        }

        var cartItem = await unitOfWork.CartItemRepository.FirstOrDefault(ci => ci.CartId == cart.Id && ci.ProductId == request.ProductId);
        if (cartItem == null)
        {
            return new ApiResponse("Product not found in cart");
        }

        var priceToRemove = request.Quantity * (cartItem.Price / cartItem.Quantity);

        cartItem.Quantity -= request.Quantity;
        if (cartItem.Quantity <= 0)
        {
            unitOfWork.CartItemRepository.Delete(cartItem);
        }
        else
        {
            cartItem.Price -= priceToRemove; 
            unitOfWork.CartItemRepository.Update(cartItem);
        }


        cart.TotalCost -= priceToRemove; 

        unitOfWork.CartRepository.Update(cart); 
        await unitOfWork.Complete();
        return new ApiResponse();
    }
}
