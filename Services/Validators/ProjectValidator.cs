using FluentValidation;
using web_app.ApiClient;

namespace web_app.Services.Validators
{
    public class ProjectValidator : AbstractValidator<ProjectResponse>
    {
        public ProjectValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre es requerido.")
                .NotNull().WithMessage("El nombre es requerido.")
                .Length(1, 100).WithMessage("El nombre debe contener entre 1 y 100 caracteres");

            RuleFor(x => x.Description)
                .MaximumLength(250).WithMessage("La descripción debe hasta 250 caracteres");
            
            RuleFor(x => x.Location)
                .MaximumLength(250).WithMessage("La localización debe contener hasta 50 caracteres");

            RuleFor(x => x.StartDate)
                .NotNull().WithMessage("La fecha de inicio es obligatoria.")
                .NotEmpty().WithMessage("La fecha es obligatoria.")
                .Must(BeAValidDate).WithMessage("La fecha debe ser válida.");

            RuleFor(x => x.EndDate)
                .NotNull().WithMessage("La fecha de finalización es obligatoria.")
                .Must(BeAValidDate).WithMessage("La fecha de finalización debe ser válida.")
                .GreaterThan(x => x.StartDate).WithMessage("La fecha de finalización debe ser mayor a la de inicio.");

        }

        private bool BeAValidDate(DateTime? date)
        {
            return date.HasValue && date.Value != default;
        }

        public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
        {
            var result =
                await ValidateAsync(ValidationContext<ProjectResponse>.CreateWithOptions((ProjectResponse)model,
                    x => x.IncludeProperties(propertyName)));
            if (result.IsValid)
                return Array.Empty<string>();
            return result.Errors.Select(e => e.ErrorMessage);
        };
    }
}