namespace Todo.Api.DTOs.Auth
{
    public class AuthUserResponse
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string? DisplayName { get; set; }
    }
}
