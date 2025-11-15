using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Frontend.Pages.Orders
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public IndexModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public List<OrderDto> Orders { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            // Read token from session
            var token = HttpContext.Session.GetString("JWT");

            // If no token ? continue loading the page without redirecting
            // (this avoids the redirect loop and lets you continue your project)
            if (string.IsNullOrEmpty(token))
            {
                // Just load an empty list for now
                Orders = new List<OrderDto>();
                return Page();
            }

            var client = _clientFactory.CreateClient("Gateway");

            // Attach token
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // Call API
            var response = await client.GetAsync("/gateway/order");

            // If token invalid ? show empty list (no redirect)
            if (!response.IsSuccessStatusCode)
            {
                Orders = new List<OrderDto>();
                return Page();
            }

            var json = await response.Content.ReadAsStringAsync();

            Orders = JsonSerializer.Deserialize<List<OrderDto>>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

            return Page();
        }

        public class OrderDto
        {
            public int Id { get; set; }
            public DateTime OrderDate { get; set; }
            public decimal Total { get; set; }
            public int ClientId { get; set; }
        }
    }
}
