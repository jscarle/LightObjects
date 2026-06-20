using System.ComponentModel;
using System.Text.Json;
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

    [Fact]
    public void Json_GenericIdentifier_ShouldRoundTrip()
    {
        var id = TestGenericIntId<string>.Create(42);

        var json = JsonSerializer.Serialize(id);
        var deserialized = JsonSerializer.Deserialize<TestGenericIntId<string>>(json);

        json.ShouldBe("42");
        deserialized.ShouldBe(id);
    }

    [Fact]
    public void TypeConverter_GenericStructIdentifier_ShouldNotThrow()
    {
        var converter = TypeDescriptor.GetConverter(typeof(TestGenericIntId<string>));

        converter.ShouldNotBeNull();
    }

    [Fact]
    public void TypeConverter_GenericStringIdentifier_ShouldConvertFromString()
    {
        var converter = TypeDescriptor.GetConverter(typeof(TestGenericStringId<string>));

        var id = converter.ConvertFrom("ABC-123");

        converter.CanConvertFrom(typeof(string)).ShouldBeTrue();
        id.ShouldBe(TestGenericStringId<string>.Create("ABC-123"));
    }

    [Fact]
    public void Create_NestedIdentifierInGenericContainer_ShouldSucceed()
    {
        var id = TestGenericIdentifierContainer<string>.TestNestedGenericContainerIntId.Create(42);

        id.ToInt32().ShouldBe(42);
    }

    [Fact]
    public void Json_NestedIdentifierInGenericContainer_ShouldRoundTrip()
    {
        var id = TestGenericIdentifierContainer<string>.TestNestedGenericContainerIntId.Create(42);

        var json = JsonSerializer.Serialize(id);
        var deserialized = JsonSerializer.Deserialize<TestGenericIdentifierContainer<string>.TestNestedGenericContainerIntId>(json);

        json.ShouldBe("42");
        deserialized.ShouldBe(id);
    }

    [Fact]
    public void Create_GenericIdentifierWithNullableReferenceConstraint_ShouldSucceed()
    {
        var id = TestNullableReferenceGenericIntId<string?>.Create(42);

        id.ToInt32().ShouldBe(42);
    }
}
