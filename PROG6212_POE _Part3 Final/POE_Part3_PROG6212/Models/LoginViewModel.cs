namespace POE_Part2_PROG6212.Models
{
    public class LoginViewModel
    {
        public string? ReturnUrl { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }
}

