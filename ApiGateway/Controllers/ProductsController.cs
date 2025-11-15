using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace ApiGateway.Controllers
{
    [ApiController]
    [Route("api/product")]
    public class ProductsController : ControllerBase
    {
        private readonly HttpClient _client;

        public ProductsController(IHttpClientFactory factory)
        {
            _client = factory.CreateClient("ProductService");
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var products = await _client.GetFromJsonAsync<object>("api/product");
            return Ok(products);
        }
    }
}
