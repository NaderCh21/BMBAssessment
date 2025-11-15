using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using OrderService.Domain.DTO;
using OrderService.Domain.Entities;
using OrderService.Domain.Interfaces;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IMapper _mapper;
        private readonly HttpClient _httpClient;
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            IOrderService orderService,
            IMapper mapper,
            IHttpClientFactory httpClientFactory,
            ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _mapper = mapper;
            _httpClient = httpClientFactory.CreateClient("productService");
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _orderService.GetAllAsync();
            return Ok(orders);
        }


        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order == null)
                return NotFound();

            return Ok(order);
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
        {
            // Validate product
            var productResponse = await _httpClient.GetAsync($"/api/product/{dto.ProductId}");

            if (!productResponse.IsSuccessStatusCode)
                return BadRequest("Invalid ProductId — product does not exist.");

            var productData = await productResponse.Content.ReadFromJsonAsync<ProductResponse>();

            if (productData == null)
                return BadRequest("Cannot read product information.");

            // Calculate total
            decimal total = productData.Price * dto.Quantity;

            // Create order entity
            var order = new Order
            {
                ProductId = dto.ProductId,
                ClientId = dto.ClientId, 
                Quantity = dto.Quantity,
                OrderDate = dto.OrderDate,
                Total = total,
                CreatedByEmployeeId = "EMP001"
            };

            var createdOrder = await _orderService.CreateAsync(order);

            return CreatedAtAction(nameof(GetById), new { id = createdOrder.Id }, createdOrder);
        }




        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateOrderDto dto)
        {
            if (id != dto.Id)
                return BadRequest("URL id does not match DTO id.");

            var existing = await _orderService.GetByIdAsync(id);
            if (existing == null)
                return NotFound("Order not found.");

            // Validate product
            var product = await _httpClient.GetAsync($"/api/product/{dto.ProductId}");
            if (!product.IsSuccessStatusCode)
                return BadRequest("Invalid ProductId.");

            var productData = await product.Content.ReadFromJsonAsync<ProductResponse>();
            if (productData == null)
                return BadRequest("Cannot read product information.");

            _mapper.Map(dto, existing);

            existing.Total = productData.Price * dto.Quantity;

            var updated = await _orderService.UpdateAsync(existing);

            if (!updated)
                return StatusCode(500, "Update failed.");

            return Ok(existing);
        }


        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _orderService.DeleteAsync(id);

            if (!deleted)
                return NotFound("Order not found.");

            return NoContent();
        }
    }

    public class ProductResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
    }
}
