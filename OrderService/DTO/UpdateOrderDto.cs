namespace OrderService.Domain.DTO
{
    public class UpdateOrderDto
    {
        public int Id { get; set; }         
        public int ProductId { get; set; }     
        public int ClientId { get; set; }
        public int Quantity { get; set; }
        public DateTime OrderDate { get; set; }
    }
}
