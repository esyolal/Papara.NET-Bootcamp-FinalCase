using MediatR;
using Pp.Base.Response;
using Pp.Schema;

namespace Pp.Business.Cqrs
{
    public record AddFundsToWalletCommand(long userId,decimal Amount) : IRequest<ApiResponse<WalletResponse>>;

    public record GetWalletByUserIdQuery(long UserId) : IRequest<ApiResponse<WalletResponse>>;

    public record GetAllWalletsQuery() : IRequest<ApiResponse<List<WalletResponse>>>;

    public record UpdateWalletBalanceCommand(long UserId,decimal NewBalance) : IRequest<ApiResponse<WalletResponse>>;
}
