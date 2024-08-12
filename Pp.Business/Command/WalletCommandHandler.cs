using AutoMapper;
using MediatR;
using Pp.Base.Response;
using Pp.Business.Cqrs;
using Pp.Business.Services;
using Pp.Data.UnitOfWork;
using Pp.Schema;

namespace Pp.Business.Command
{
    public class WalletCommandHandler :
        IRequestHandler<AddFundsToWalletCommand, ApiResponse<WalletResponse>>,
        IRequestHandler<UpdateWalletBalanceCommand, ApiResponse<WalletResponse>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly ICurrentUser currentUser;

        public WalletCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUser currentUser)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.currentUser = currentUser;
        }

        public async Task<ApiResponse<WalletResponse>> Handle(AddFundsToWalletCommand request, CancellationToken cancellationToken)
        {
            var userId = currentUser.GetCurrentUserId();

            var wallet = await unitOfWork.WalletRepository.FirstOrDefault(w => w.UserId == userId);
            if (wallet == null)
            {
                return new ApiResponse<WalletResponse>("Wallet not found");
            }

            wallet.Balance += request.Amount;
            unitOfWork.WalletRepository.Update(wallet);
            await unitOfWork.Complete();

            var response = mapper.Map<WalletResponse>(wallet);
            return new ApiResponse<WalletResponse>(response);
        }

        public async Task<ApiResponse<WalletResponse>> Handle(UpdateWalletBalanceCommand request, CancellationToken cancellationToken)
        {
            var wallet = await unitOfWork.WalletRepository.FirstOrDefault(w => w.UserId == request.UserId);
            if (wallet == null)
            {
                return new ApiResponse<WalletResponse>("Wallet not found");
            }

            wallet.Balance = request.NewBalance;
            unitOfWork.WalletRepository.Update(wallet);
            await unitOfWork.Complete();

            var response = mapper.Map<WalletResponse>(wallet);
            return new ApiResponse<WalletResponse>(response);
        }
    }
}
