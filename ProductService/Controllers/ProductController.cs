using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ProductService.Domain.Entities;
using ProductService.Interfaces;
using Serilog;
using ProductService.DTOs;

namespace ProductService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IMapper _mapper;
        private readonly Serilog.ILogger _logger = Log.ForContext<ProductController>();

        public ProductController(IProductService productService, IMapper mapper)
        {
            _productService = productService;
            _mapper = mapper;
        }

        // GET: api/product
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                _logger.Information("Fetching all products");

                var products = await _productService.GetAllAsync();
                var result = _mapper.Map<IEnumerable<ProductDto>>(products);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error occurred while fetching all products");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/product/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                _logger.Information("Fetching product with ID {ProductId}", id);

                var product = await _productService.GetByIdAsync(id);
                if (product == null)
                    return NotFound($"Product with ID {id} not found");

                return Ok(_mapper.Map<ProductDto>(product));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error fetching product with ID {ProductId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/product
        [HttpPost]
        public async Task<IActionResult> Create(CreateProductDto dto)
        {
            try
            {
                _logger.Information("Creating new product");

                var product = _mapper.Map<Product>(dto);
                var created = await _productService.CreateAsync(product);

                var response = _mapper.Map<ProductDto>(created);

                return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error creating product");
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT: api/product/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpdateProductDto dto)
        {
            try
            {
                _logger.Information("Updating product with ID {ProductId}", id);

                var existing = await _productService.GetByIdAsync(id);
                if (existing == null)
                    return NotFound($"Product with ID {id} not found");

                _mapper.Map(dto, existing);

                var success = await _productService.UpdateAsync(existing);

                if (!success)
                    return StatusCode(500, "Could not update product");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating product with ID {ProductId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE: api/product/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.Information("Deleting product with ID {ProductId}", id);

                var success = await _productService.DeleteAsync(id);

                if (!success)
                {
                    return NotFound($"Product with ID {id} not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error deleting product with ID {ProductId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
