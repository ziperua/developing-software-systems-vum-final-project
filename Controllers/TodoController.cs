using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Todo.Api.DTOs.Todos;
using Todo.Api.Services;

namespace Todo.Api.Controllers
{
    [ApiController]
    [Route("api/todos")]
    [Authorize]
    public class TodoController : ControllerBase
    {
        private readonly TodoService _todoService;

        public TodoController(TodoService todoService)
        {
            _todoService = todoService;
        }
        private Guid GetUserId()
        {
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        [HttpGet("public")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublicTodos([FromQuery] TodoQueryParams queryParams)
        {
            var result = await _todoService.GetPublicTodos(queryParams);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetTodos([FromQuery] TodoQueryParams queryParams)
        {
            var result = await _todoService.GetTodos(GetUserId(), queryParams);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTodo([FromBody] CreateTodoRequest request)
        {
            var result = await _todoService.CreateTodo(request, GetUserId());
            return StatusCode(201, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _todoService.GetById(id, GetUserId());
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTodo([FromBody] UpdateTodoRequest request, Guid id)
        {
            var result = await _todoService.UpdateTodo(request, id, GetUserId());
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPatch("{id}/completion")]
        public async Task<IActionResult> SetCompletion([FromBody] SetCompletionRequest request, Guid id)
        {
            var result = await _todoService.SetCompletion(id, request.IsCompleted, GetUserId());
            if (result == null) return NotFound();
            return Ok(result);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodo(Guid id)
        {
            var result = await _todoService.DeleteTodo(id, GetUserId());
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
