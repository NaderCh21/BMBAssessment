using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Frontend.Pages.Orders
{
    public class DeleteModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public DeleteModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [BindProperty]
        public int OrderId { get; set; }

        public string ClientName { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal Total { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            OrderId = id;

            var client = _clientFactory.CreateClient("Gateway");

            var response = await client.GetAsync($"/api/order/{id}");
            if (!response.IsSuccessStatusCode)
                return RedirectToPage("Index");

            var json = await response.Content.ReadAsStringAsync();

            var dto = JsonSerializer.Deserialize<OrderDto>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            ClientName = dto.ClientName;
            OrderDate = dto.OrderDate;
            Total = dto.Total;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            OrderId = id;

            var client = _clientFactory.CreateClient("Gateway");

            var response = await client.DeleteAsync($"/api/order/{id}");

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Failed to delete order.");
                return Page();
            }

            return RedirectToPage("Index");
        }

        // DTO for delete confirmation
        public class OrderDto
        {
            public int Id { get; set; }
            public int ClientId { get; set; }
            public string ClientName { get; set; }
            public DateTime OrderDate { get; set; }
            public decimal Total { get; set; }

            // Optionally include product list if needed:
            public List<OrderProduct> Products { get; set; }
        }

        public class OrderProduct
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
        }
    }
}
