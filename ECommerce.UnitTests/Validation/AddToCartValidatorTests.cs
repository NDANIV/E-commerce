using ECommerce.Application.Cart.Commands;
using FluentValidation.TestHelper;
using Xunit;

namespace ECommerce.UnitTests.Validation;

/// <summary>Validaciones básicas de AddToCart.</summary>
public sealed class AddToCartValidatorTests
{
    [Fact]
    public void Falla_si_quantity_es_menor_o_igual_a_cero()
    {
        var v = new AddToCartValidator();

        v.TestValidate(new AddToCartCommand(Guid.NewGuid(), Guid.NewGuid(), 0))
         .ShouldHaveValidationErrorFor(x => x.Quantity);

        v.TestValidate(new AddToCartCommand(Guid.NewGuid(), Guid.NewGuid(), -1))
         .ShouldHaveValidationErrorFor(x => x.Quantity);
    }
}
