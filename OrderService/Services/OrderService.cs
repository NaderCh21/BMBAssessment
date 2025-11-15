using OrderService.Domain.Entities;
using OrderService.Domain.Interfaces;

namespace OrderService.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _repository;
        private readonly HttpClient _httpClient;

        public OrderService(IOrderRepository repository, HttpClient httpClient)
        {
            _repository = repository;
            _httpClient = httpClient;
        }

        // 🔍 Validate that the product exists in ProductService before creating an order
        private async Task<bool> IsProductValid(int productId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"http://localhost:6001/api/Product/{productId}");
                return response.IsSuccessStatusCode;  
            }
            catch
            {
                return false;  // ProductService unreachable
            }
        }

        public Task<IEnumerable<Order>> GetAllAsync() =>
            _repository.GetAllAsync();

        public Task<Order?> GetByIdAsync(int id) =>
            _repository.GetByIdAsync(id);

        public async Task<Order> CreateAsync(Order order)
        {
            // Validate ProductId via ProductService
            bool isValidProduct = await IsProductValid(order.ProductId);

            if (!isValidProduct)
                throw new Exception("Invalid ProductId: Product does not exist in ProductService.");

            return await _repository.CreateAsync(order);
        }

        public Task<bool> UpdateAsync(Order order) =>
            _repository.UpdateAsync(order);

        public Task<bool> DeleteAsync(int id) =>
            _repository.DeleteAsync(id);
    }
}
