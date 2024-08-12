using AutoMapper;
using MediatR;
using Pp.Base.Response;
using Pp.Business.Cqrs;
using Pp.Data.UnitOfWork;
using Pp.Schema;

namespace Pp.Business.Query
{
    public class WalletQueryHandler :
        IRequestHandler<GetWalletByUserIdQuery, ApiResponse<WalletResponse>>,
        IRequestHandler<GetAllWalletsQuery, ApiResponse<List<WalletResponse>>>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public WalletQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<ApiResponse<WalletResponse>> Handle(GetWalletByUserIdQuery request, CancellationToken cancellationToken)
        {
            var wallet = await unitOfWork.WalletRepository.FirstOrDefault(w => w.UserId == request.UserId);
            if (wallet == null)
            {
                return new ApiResponse<WalletResponse>("Wallet not found");
            }

            var response = mapper.Map<WalletResponse>(wallet);
            return new ApiResponse<WalletResponse>(response);
        }

        public async Task<ApiResponse<List<WalletResponse>>> Handle(GetAllWalletsQuery request, CancellationToken cancellationToken)
        {
            var wallets = await unitOfWork.WalletRepository.GetAll();
            var response = mapper.Map<List<WalletResponse>>(wallets);
            return new ApiResponse<List<WalletResponse>>(response);
        }
    }
}
