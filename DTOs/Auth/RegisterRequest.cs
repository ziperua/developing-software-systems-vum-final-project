namespace Todo.Api.DTOs.Auth
{
    public class RegisterRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string? DisplayName { get; set; }
    }
}
