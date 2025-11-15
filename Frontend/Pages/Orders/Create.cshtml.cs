using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Frontend.Pages.Orders
{
    [IgnoreAntiforgeryToken]
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public CreateModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        // -------- FORM PROPERTIES ----------
        [BindProperty]
        public int ClientId { get; set; }  // dummy clients are strings

        [BindProperty]
        public DateTime OrderDate { get; set; } = DateTime.Today;

        [BindProperty]
        public int ProductId { get; set; }

        [BindProperty]
        public int Quantity { get; set; }

         public List<DummyClient> DummyClients { get; set; } = new()
        {
            new DummyClient { Id = 1, Name = "Client A" },
            new DummyClient { Id = 2, Name = "Client B" },
            new DummyClient { Id = 3, Name = "Client C" },
            new DummyClient { Id = 4, Name = "Client D" }
        };
         public List<ProductDto> Products { get; set; } = new();

        // GET
        public async Task<IActionResult> OnGetAsync()
        {
            await LoadProducts();
            return Page();
        }

        // POST
        public async Task<IActionResult> OnPostAsync()
        {
            await LoadProducts();

            // VALIDATION
            if (ClientId <= 0)
                ModelState.AddModelError("ClientId", "Client is required.");

            if (ProductId <= 0)
                ModelState.AddModelError("ProductId", "Product is required.");

            if (Quantity <= 0)
                ModelState.AddModelError("Quantity", "Quantity must be greater than zero.");

            if (!ModelState.IsValid)
                return Page();

            var client = _clientFactory.CreateClient("Gateway");

            // This MUST match CreateOrderDto in backend
            var order = new
            {
                ClientId = ClientId,
                ProductId = ProductId,
                Quantity = Quantity,
                OrderDate = OrderDate
            };

            var response = await client.PostAsJsonAsync("/api/order", order);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Failed to create order.");
                return Page();
            }

            return RedirectToPage("Index");
        }

        // LOAD PRODUCT DROPDOWN
        private async Task LoadProducts()
        {
            var client = _clientFactory.CreateClient("Gateway");
            var response = await client.GetAsync("/api/product");

            if (response.IsSuccessStatusCode)
            {
                Products = await response.Content.ReadFromJsonAsync<List<ProductDto>>();
            }
        }

        // DTOs
        public class DummyClient
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class ProductDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class ValidationErrorResponse
        {
            public Dictionary<string, string[]> Errors { get; set; }
        }
    }
}
