using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace ApiGateway.Controllers
{
    [ApiController]
    [Route("gateway/order")]
    public class OrdersController : ControllerBase
    {
        private readonly IHttpClientFactory _clientFactory;

        public OrdersController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var client = _clientFactory.CreateClient("OrderService");

            var response = await client.GetAsync("/api/order");

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Order service unavailable");

            // Deserialize directly into List<OrderDto>
            var orders = await response.Content.ReadFromJsonAsync<List<OrderDto>>();

            return Ok(orders);
        }

        
        public class OrderDto
        {
            public int Id { get; set; }
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public decimal Total { get; set; }
            public int ClientId { get; set; }
            public DateTime OrderDate { get; set; }
            public string CreatedByEmployeeId { get; set; }
        }


        // POST: api/order
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] object order)
        {
            var client = _clientFactory.CreateClient("OrderService");

            var response = await client.PostAsJsonAsync("/api/order", order);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, error);
            }

            var created = await response.Content.ReadFromJsonAsync<object>();
            return Ok(created);
        }
    }
}
