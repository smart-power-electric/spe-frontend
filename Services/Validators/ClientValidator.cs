using FluentValidation;
using web_app.ApiClient;

namespace web_app.Services.Validators
{
    public class ClientValidator : AbstractValidator<ClientResponse>
    {
        public ClientValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre es requerido.")
                .NotNull().WithMessage("El nombre es requerido.")
                .Length(1, 100).WithMessage("El nombre debe contener entre 1 y 100 caracteres");

            RuleFor(x => x.Address)
                .Length(1, 100).WithMessage("La dirección debe contener entre 1 y 250 caracteres");

            RuleFor(x => x.Phone)
                .Matches(@"^\d+$").WithMessage("El número telefónico solo debe contener dígitos.")
                .Length(8, 15).WithMessage("El número telefónico debe tener entre 8 y 15 dígitos.");

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("El correo electrónico no es válido.")
                .Length(5, 100).WithMessage("El correo electrónico debe contener entre 3 y 100 caracteres");

        }

        public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
        {
            var result = await ValidateAsync(ValidationContext<ClientResponse>.CreateWithOptions((ClientResponse)model, x => x.IncludeProperties(propertyName)));
            if (result.IsValid)
                return Array.Empty<string>();
            return result.Errors.Select(e => e.ErrorMessage);
        };
    }
}
