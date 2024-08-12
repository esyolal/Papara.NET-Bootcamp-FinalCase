using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Pp.Base.Response;
using Pp.Business.Cqrs;
using Pp.Schema;

namespace Pp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class CategoryController : ControllerBase
    {
        private readonly IMediator mediator;

        public CategoryController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoryRequest request)
        {
            var command = new CreateCategoryCommand(request);
            var response = await mediator.Send(command);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] CategoryRequest request)
        {
            var command = new UpdateCategoryCommand(id, request);
            var response = await mediator.Send(command);
            return response.IsSuccess ? NoContent() : BadRequest(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var command = new DeleteCategoryCommand(id);
            var response = await mediator.Send(command);
            return response.IsSuccess ? NoContent() : BadRequest(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var query = new GetCategoryByIdQuery(id);
            var response = await mediator.Send(query);
            return response.IsSuccess ? Ok(response) : NotFound(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var query = new GetAllCategoriesQuery();
            var response = await mediator.Send(query);
            return response.IsSuccess ? Ok(response) : NotFound(response);
        }

        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetByProduct(long productId)
        {
            var query = new GetCategoriesByProductQuery(productId);
            var response = await mediator.Send(query);
            return response.IsSuccess ? Ok(response) : NotFound(response);
        }
    }
}
