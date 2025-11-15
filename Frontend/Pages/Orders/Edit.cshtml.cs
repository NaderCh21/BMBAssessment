using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Frontend.Pages.Orders
{
    public class EditModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public EditModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        // -------- FIELDS MATCHING UpdateOrderDto ----------
        [BindProperty]
        public int OrderId { get; set; }

        [BindProperty]
        public int ClientId { get; set; }

        [BindProperty]
        public int ProductId { get; set; }

        [BindProperty]
        public int Quantity { get; set; }

        [BindProperty]
        public DateTime OrderDate { get; set; }

        public List<ClientDto> Clients { get; set; } = new();
        public List<ProductDto> Products { get; set; } = new();

        // ---------------- READ -----------------
        public async Task<IActionResult> OnGetAsync(int id)
        {
            OrderId = id;

            await LoadDropdowns();

            var client = _clientFactory.CreateClient("Gateway");

            // FIX URL
            var response = await client.GetAsync($"/gateway/order/{id}");
            if (!response.IsSuccessStatusCode)
                return RedirectToPage("Index");

            var json = await response.Content.ReadAsStringAsync();

            var dto = JsonSerializer.Deserialize<OrderDto>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // FILL FORM FIELDS
            ClientId = dto.ClientId;
            ProductId = dto.ProductId;
            Quantity = dto.Quantity;
            OrderDate = dto.OrderDate;

            return Page();
        }

        // ---------------- UPDATE -----------------
        public async Task<IActionResult> OnPostAsync(int id)
        {
            OrderId = id;

            await LoadDropdowns();

            // VALIDATION
            if (ClientId <= 0)
                ModelState.AddModelError("ClientId", "Client is required.");

            if (ProductId <= 0)
                ModelState.AddModelError("ProductId", "Product is required.");

            if (Quantity <= 0)
                ModelState.AddModelError("Quantity", "Quantity must be > 0.");

            if (!ModelState.IsValid)
                return Page();

            var client = _clientFactory.CreateClient("Gateway");

            // THIS SHOULD MATCH UpdateOrderDto
            var order = new
            {
                Id = id,
                ClientId = ClientId,
                ProductId = ProductId,
                Quantity = Quantity,
                OrderDate = OrderDate
            };

            var response = await client.PutAsJsonAsync($"/gateway/order/{id}", order);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Failed to update order.");
                return Page();
            }

            return RedirectToPage("Index");
        }

        // --------------- DROPDOWNS -------------------
        private async Task LoadDropdowns()
        {
            var client = _clientFactory.CreateClient("Gateway");

            // dummy clients like in Create page
            Clients = new List<ClientDto>
            {
                new() { Id = 1, Name = "Client A" },
                new() { Id = 2, Name = "Client B" },
                new() { Id = 3, Name = "Client C" },
                new() { Id = 4, Name = "Client D" }
            };

            var productsResponse = await client.GetAsync("/gateway/product");
            if (productsResponse.IsSuccessStatusCode)
                Products = await productsResponse.Content.ReadFromJsonAsync<List<ProductDto>>();
        }

        // -------- DTOs ----------
        public class OrderDto
        {
            public int ClientId { get; set; }
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public DateTime OrderDate { get; set; }
        }

        public class ClientDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class ProductDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
