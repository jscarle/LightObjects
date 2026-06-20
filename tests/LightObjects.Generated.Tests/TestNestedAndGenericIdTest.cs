using LightObjects.Generated.Fixtures.Identifiers;
using Shouldly;

namespace LightObjects.Generated.Tests;

public sealed class TestNestedAndGenericIdTest
{
    [Fact]
    public void Create_NestedIdentifier_ShouldSucceed()
    {
        var id = TestIdentifierContainer.TestNestedIntId.Create(42);

        id.ToInt32().ShouldBe(42);
    }

    [Fact]
    public void Create_GenericIdentifier_ShouldSucceed()
    {
        var id = TestGenericIntId<string>.Create(42);

        id.ToInt32().ShouldBe(42);
    }
}
