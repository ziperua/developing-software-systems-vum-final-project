using Microsoft.AspNetCore.Mvc;
using Todo.Api.DTOs.Auth;
using Todo.Api.Services;

namespace Todo.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var result = await _authService.Register(request);
            if (result == null)
                return Conflict("Email already exists");
            else
                return StatusCode(201, result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var result = await _authService.Login(request);
            if (result == null)
                return Unauthorized();
            else
                return Ok(result);
        }
    }
}
