using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace Frontend.Pages.Auth
{
    [IgnoreAntiforgeryToken]
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public LoginModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [BindProperty]
        [Required]
        public string Username { get; set; }

        [BindProperty]
        [Required]
        public string Password { get; set; }

        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var client = _clientFactory.CreateClient("Gateway");

            var response = await client.PostAsJsonAsync("/auth/login", new
            {
                Username,
                Password
            });

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = "Invalid username or password.";
                return Page();
            }

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

            if (result == null || string.IsNullOrEmpty(result.Token))
            {
                ErrorMessage = "Unexpected error.";
                return Page();
            }

            // Save token in session
            HttpContext.Session.SetString("JWT", result.Token);

            // Create authentication cookie
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, Username)
            };

            var identity = new ClaimsIdentity(claims, "CookieAuth");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("CookieAuth", principal);

            // SIMPLE REDIRECT – always goes to Orders page
            return RedirectToPage("/Orders/Index");
        }

        public class LoginResponse
        {
            [JsonPropertyName("token")]
            public string Token { get; set; }
        }
    }
}
