using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
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

        [BindProperty]
        public int OrderId { get; set; }

        [BindProperty]
        public int ClientId { get; set; }

        [BindProperty]
        public DateTime OrderDate { get; set; }

        [BindProperty]
        public List<OrderProductInput> ProductsList { get; set; } = new();

        public List<ClientDto> Clients { get; set; } = new();
        public List<ProductDto> Products { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            OrderId = id;

            await LoadDropdowns();

            var client = _clientFactory.CreateClient("Gateway");

            var response = await client.GetAsync($"/api/order/{id}");
            if (!response.IsSuccessStatusCode)
                return RedirectToPage("Index");

            var json = await response.Content.ReadAsStringAsync();

            var dto = JsonSerializer.Deserialize<OrderDto>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            ClientId = dto.ClientId;
            OrderDate = dto.OrderDate;
            ProductsList = dto.Products;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            OrderId = id;

            await LoadDropdowns();

            if (ClientId <= 0)
                ModelState.AddModelError("ClientId", "Client is required.");

            if (ProductsList.Count == 0)
                ModelState.AddModelError("", "At least one product is required.");

            if (!ModelState.IsValid)
                return Page();

            var client = _clientFactory.CreateClient("Gateway");

            var order = new
            {
                ClientId,
                OrderDate,
                Products = ProductsList
            };

            var response = await client.PutAsJsonAsync($"/api/order/{id}", order);

            if (!response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();

                try
                {
                    var errorObj = JsonSerializer.Deserialize<ValidationErrorResponse>(json);

                    if (errorObj?.Errors != null)
                    {
                        foreach (var kvp in errorObj.Errors)
                        {
                            foreach (var msg in kvp.Value)
                            {
                                ModelState.AddModelError(kvp.Key, msg);
                            }
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Update failed.");
                    }
                }
                catch
                {
                    ModelState.AddModelError("", "Unexpected error.");
                }

                return Page();
            }

            return RedirectToPage("Index");
        }

        private async Task LoadDropdowns()
        {
            var client = _clientFactory.CreateClient("Gateway");

            var clientsResponse = await client.GetAsync("/api/clients");
            if (clientsResponse.IsSuccessStatusCode)
                Clients = await clientsResponse.Content.ReadFromJsonAsync<List<ClientDto>>();

            var productsResponse = await client.GetAsync("/api/products");
            if (productsResponse.IsSuccessStatusCode)
                Products = await productsResponse.Content.ReadFromJsonAsync<List<ProductDto>>();
        }

        public class OrderDto
        {
            public int ClientId { get; set; }
            public DateTime OrderDate { get; set; }
            public List<OrderProductInput> Products { get; set; }
        }

        public class OrderProductInput
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
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

        public class ValidationErrorResponse
        {
            public Dictionary<string, string[]> Errors { get; set; }
        }
    }
}
