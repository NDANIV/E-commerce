using FluentValidation;

namespace ECommerce.Application.Products.Commands;

/// <summary>
/// Reglas de validación para la creación de productos.
/// </summary>
public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        // Nombre: obligatorio, 3-100 caracteres
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MinimumLength(3).WithMessage("El nombre debe tener al menos 3 caracteres.")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres.");

        // Precio: mayor o igual a 0
        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("El precio no puede ser negativo.");

        // Stock: mayor o igual a 0
        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("El stock no puede ser negativo.");

        // CategoryId: no vacío
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Debe indicar una categoría válida.");
    }
}