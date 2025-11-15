namespace OrderService.Domain.DTO
{
    public class CreateOrderDto
    {
        public int ProductId { get; set; }
        public int ClientId { get; set; }
        public int Quantity { get; set; }
        public DateTime OrderDate { get; set; }
    }
}
