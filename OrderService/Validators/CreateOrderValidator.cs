using FluentValidation;
using OrderService.Domain.DTO;

namespace OrderService.Validators
{
    public class CreateOrderValidator : AbstractValidator<CreateOrderDto>
    {
        public CreateOrderValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("ProductId must be greater than 0.");

            RuleFor(x => x.ClientId)
                .GreaterThan(0).WithMessage("ClientId must be greater than 0.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.");

            RuleFor(x => x.OrderDate)
                .LessThanOrEqualTo(DateTime.Now)
                .WithMessage("Order date cannot be in the future.");
        }
    }
}
