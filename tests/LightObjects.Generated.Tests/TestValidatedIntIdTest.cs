using LightObjects.Generated.Fixtures.Identifiers;
using Shouldly;

namespace LightObjects.Generated.Tests;

public sealed class TestValidatedIntIdTest
{
    [Fact]
    public void Create_ValidValue_ShouldSucceed()
    {
        var id = TestValidatedIntId.Create(42);

        id.ToInt32()
            .ShouldBe(42);
    }

    [Fact]
    public void Create_InvalidValue_ShouldThrowException()
    {
        Func<object?> create = () => TestValidatedIntId.Create(0);

        var exception = Should.Throw<ValueObjectException>(create);
        exception.Message.ShouldBe("The value must be greater than zero.");
    }

    [Fact]
    public void TryCreate_InvalidValue_ShouldFail()
    {
        var result = TestValidatedIntId.TryCreate(0);

        result.IsFailure(out var error)
            .ShouldBeTrue();
        error.Message.ShouldBe("The value must be greater than zero.");
    }

    [Fact]
    public void TryParse_InvalidValue_ShouldFail()
    {
        var result = TestValidatedIntId.TryParse("0");

        result.IsFailure(out var error)
            .ShouldBeTrue();
        error.Message.ShouldBe("The value must be greater than zero.");
    }
}
