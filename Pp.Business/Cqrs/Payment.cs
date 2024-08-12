using MediatR;
using Pp.Base.Response;
namespace Pp.Business.Cqrs;
public class ProcessPaymentCommand : IRequest<ApiResponse>
{
    public long CartId { get; set; }
    public bool UsePoints { get; set; }
}
