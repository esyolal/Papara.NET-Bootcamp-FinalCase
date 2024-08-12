using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Pp.Business.Cqrs;
using Pp.Schema;

namespace Pp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ProductController : ControllerBase
    {
        private readonly IMediator mediator;

        public ProductController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductRequest request)
        {
            var command = new CreateProductCommand(request);
            var response = await mediator.Send(command);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] ProductRequest request)
        {
            var command = new UpdateProductCommand(id, request);
            var response = await mediator.Send(command);
            return response.IsSuccess ? NoContent() : BadRequest(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var command = new DeleteProductCommand(id);
            var response = await mediator.Send(command);
            return response.IsSuccess ? NoContent() : BadRequest(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var query = new GetProductByIdQuery(id);
            var response = await mediator.Send(query);
            return response.IsSuccess ? Ok(response) : NotFound(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var query = new GetAllProductsQuery();
            var response = await mediator.Send(query);
            return response.IsSuccess ? Ok(response) : NotFound(response);
        }

        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetByCategory(long categoryId)
        {
            var query = new GetProductsByCategoryQuery(categoryId);
            var response = await mediator.Send(query);
            return response.IsSuccess ? Ok(response) : NotFound(response);
        }
    }
}
