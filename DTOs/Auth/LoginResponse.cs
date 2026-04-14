namespace Todo.Api.DTOs.Auth
{
    public class LoginResponse
    {
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public int ExpiresInSeconds { get; set; }
        public AuthUserResponse User { get; set; }
    }
}
