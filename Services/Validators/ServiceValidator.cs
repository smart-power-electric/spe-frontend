using FluentValidation;
using web_app.ApiClient;

namespace web_app.Services.Validators
{
    public class ServiceValidator : AbstractValidator<ServiceResponse>
    {
        public ServiceValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre es requerido.")
                .NotNull().WithMessage("El nombre es requerido.")
                .Length(1, 100).WithMessage("El nombre debe contener entre 1 y 100 caracteres");

            RuleFor(x => x.UnitCost)
                .NotNull().WithMessage("El costo unitario es requerido.")
                .NotEmpty().WithMessage("El costo unitario es requerido.")
                .Must(BeAValidDouble).WithMessage("El costo unitario debe ser un número válido.");

            RuleFor(x => x.Description)
                .Length(1, 250).WithMessage("La descripción debe contener entre 1 y 250 caracteres");

        }

        private bool BeAValidDouble(string value)
        {
            return double.TryParse(value, out _);
        }

        private bool BeAValidDate(DateTime? date)
        {
            return date.HasValue && date.Value != default;
        }

        public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
        {
            var result = await ValidateAsync(ValidationContext<ServiceResponse>.CreateWithOptions((ServiceResponse)model, x => x.IncludeProperties(propertyName)));
            if (result.IsValid)
                return Array.Empty<string>();
            return result.Errors.Select(e => e.ErrorMessage);
        };
    }
}
