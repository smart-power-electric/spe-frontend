using FluentValidation;
using web_app.ApiClient;

namespace web_app.Services.Validators
{
    public class WorkerValidator : AbstractValidator<WorkerResponse>
    {
        public WorkerValidator()
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

            RuleFor(x => x.Speciality)
                .MaximumLength(100).WithMessage("La especialidad debe contener menos de 50 caracteres.");

            RuleFor(x => x.StartDate)
                .NotNull().WithMessage("La fecha de inicio es obligatoria.")
                .NotEmpty().WithMessage("La fecha es obligatoria.")
                .Must(BeAValidDate).WithMessage("La fecha debe ser válida.");

            RuleFor(x => x.SocialSecurity)
                .NotEmpty().WithMessage("El seguro social es requerido.")
                .NotNull().WithMessage("El seguro social es requerido.")
                .Length(1, 100).WithMessage("El seguro social debe contener entre 1 y 20 caracteres");

            RuleFor(x => x.EndDate)
                .NotNull().WithMessage("La fecha de finalización es obligatoria.")
                .Must(BeAValidDate).WithMessage("La fecha de finalización debe ser válida.")
                .GreaterThan(x => x.StartDate).WithMessage("La fecha de finalización debe ser mayor a la de inicio.");

        }

        private bool BeAValidDate(DateTimeOffset? date)
        {
            return date.HasValue && date.Value != default;
        }

        public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
        {
            var result = await ValidateAsync(ValidationContext<WorkerResponse>.CreateWithOptions((WorkerResponse)model, x => x.IncludeProperties(propertyName)));
            if (result.IsValid)
                return Array.Empty<string>();
            return result.Errors.Select(e => e.ErrorMessage);
        };
    }
}
