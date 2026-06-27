using FluentValidation;

namespace WebAPIDevSecOps.Dto
{
    public class LoginValidator : AbstractValidator<LoginRequest>
    {
        public LoginValidator()
        {
            RuleFor(x => x.User)
                .NotEmpty().WithMessage("El usuario es requerido.")
                .MaximumLength(50).WithMessage("El usuario no debe exceder 50 caracteres.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es requerida.")
                .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
                .MaximumLength(100).WithMessage("La contraseña no debe exceder 100 caracteres.")
                .Matches(@"[A-Z]").WithMessage("La contraseña debe contener al menos una letra mayúscula.")
                .Matches(@"[a-z]").WithMessage("La contraseña debe contener al menos una letra minúscula.")
                .Matches(@"[0-9]").WithMessage("La contraseña debe contener al menos un dígito.")
                .Matches(@"[\W_]").WithMessage("La contraseña debe contener al menos un carácter especial.");
        }
    }
}
