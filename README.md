[![Banner](https://raw.githubusercontent.com/jscarle/LightObjects/develop/Banner.png)](https://github.com/jscarle/LightObjects)

# LightObjects - Value Objects and Strongly Typed Identifiers for .NET

LightObjects is an extremely light and modern .NET library that provides small interfaces,
helpers, and a source generator for building value objects and strongly typed identifiers.
It is designed for applications that want explicit domain types without adding unnecessary
runtime overhead or allocation-heavy abstractions.

[![test](https://img.shields.io/github/actions/workflow/status/jscarle/LightObjects/test.yml?logo=github)](https://github.com/jscarle/LightObjects)
[![nuget](https://img.shields.io/nuget/v/LightObjects)](https://www.nuget.org/packages/LightObjects)
[![downloads](https://img.shields.io/nuget/dt/LightObjects)](https://www.nuget.org/packages/LightObjects)

## References

This library currently targets .NET 8.0, .NET 9.0, and .NET 10.0. The source generator is included
in the `LightObjects` package and is delivered as a Roslyn analyzer.

## Installation

Install the library from NuGet:

```bash
dotnet add package LightObjects
```

The old `LightObjects.Generated` NuGet package is deprecated. Source generator support is now included
in the single `LightObjects` package.

## Dependencies

This library depends on [LightResults](https://www.nuget.org/packages/LightResults) for creation,
parsing, and conversion results. No separate source generator package is required.

## Advantages of this library

- Lightweight - Only contains what's necessary to define value object contracts and generated identifiers.
- Explicit - Strongly typed identifiers prevent accidentally mixing unrelated primitive values.
- Generated - Common identifier behavior can be generated from a single partial type declaration.
- Immutable - Generated structs are readonly and generated classes expose no mutable state.
- Modern - Built against the latest version of .NET using static abstract interface members.
- Native - Written, compiled, and tested against current .NET releases.
- Compatible - Multi-targeted for current LTS and STS releases.
- Trimmable - The runtime library is compatible with ahead-of-time compilation (AOT).
- Performant - Generated code uses direct value comparisons, ordinal string comparisons, and minimal allocations.

## Getting Started

LightObjects centers on value object contracts and generated strongly typed identifiers.

- The `IValueObject<TValue, TSelf>` interface exposes the underlying value contract.
- The `ICreatableValueObject<TValue, TSelf>` interface defines `Create` and `TryCreate`.
- The `IParsableValueObject<TSelf>` interface defines `Parse` and `TryParse`.
- The `IConvertibleValueObject<TSource, TSelf>` interface defines `Convert` and `TryConvert`.
- The `ICloneableValueObject<TSelf>` interface defines `Clone`.
- The `[GeneratedIdentifier<T>]` attribute generates strongly typed identifier implementations.

### Creating a generated identifier

Add the `GeneratedIdentifier` attribute to a partial struct or class.

```csharp
using LightObjects.Generated;

namespace MyProject.Identifiers;

[GeneratedIdentifier<Guid>]
public readonly partial struct CustomerId;
```

The generator supports `short`, `int`, `long`, `string`, and `Guid` identifiers.

### Creating identifiers

Generated identifiers expose `Create` and `TryCreate`.

```csharp
var customerId = CustomerId.Create(Guid.NewGuid());

var result = CustomerId.TryCreate(Guid.NewGuid());
if (result.IsSuccess(out var identifier, out var error))
{
    Console.WriteLine(identifier);
}
else
{
    Console.WriteLine(error.Message);
}
```

`Create` throws a `ValueObjectException` when validation fails. `TryCreate` returns a
`Result<TIdentifier>` so failures can be handled without exceptions.

### Defining static identifier values

Because generated identifiers are partial types, you can add well-known static values directly to
the user-authored declaration. Initialize each value through the generated `Create` method.

```csharp
using LightObjects.Generated;

namespace MyProject.Identifiers;

[GeneratedIdentifier<int>]
public readonly partial struct StatusId
{
    public static StatusId Pending { get; } = Create(1);
    public static StatusId Enabled { get; } = Create(2);
    public static StatusId Disabled { get; } = Create(3);
    public static StatusId Archived { get; } = Create(4);
}
```

When the identifier mirrors an enum or lookup table, cast the enum value to the underlying identifier
type.

```csharp
public enum Status
{
    Pending = 1,
    Enabled = 2,
    Disabled = 3,
    Archived = 4,
}

[GeneratedIdentifier<int>]
public readonly partial struct StatusId
{
    public static StatusId Pending { get; } = Create((int)Status.Pending);
    public static StatusId Enabled { get; } = Create((int)Status.Enabled);
    public static StatusId Disabled { get; } = Create((int)Status.Disabled);
    public static StatusId Archived { get; } = Create((int)Status.Archived);

    public static IReadOnlyList<StatusId> All { get; } =
    [
        Pending,
        Enabled,
        Disabled,
        Archived,
    ];
}
```

This keeps call sites strongly typed while still making fixed database, enum, or lookup identifiers
easy to reuse.

### Parsing identifiers

Generated non-string identifiers expose `Parse` and `TryParse`.

```csharp
var customerId = CustomerId.Parse("9b6f1bc8-51f2-4f2d-b48e-3ff1a6ed95e9");

if (CustomerId.TryParse(input, out var parsedCustomerId))
{
    Console.WriteLine(parsedCustomerId);
}
```

The `TryParse(string)` overload returns a `Result<TIdentifier>` when you want the failure message.

```csharp
var result = CustomerId.TryParse(input);
if (result.IsFailure(out var error))
{
    Console.WriteLine(error.Message);
}
```

### Creating string identifiers

String identifiers must be declared as classes. The generator reports a warning for
`[GeneratedIdentifier<string>]` structs because the default value of a string-backed struct can hold
`null`. String identifiers validate that the value is not null, empty, or whitespace.

```csharp
using LightObjects.Generated;

namespace MyProject.Identifiers;

[GeneratedIdentifier<string>]
public sealed partial class ProductCode;
```

```csharp
var productCode = ProductCode.Create("ABC-123");
```

### Custom validation

Generated identifiers can use custom validation. Add a `Validate` method to the partial identifier
type with this exact signature:

```csharp
private static Result Validate(TValue value)
```

The method name and casing, `private` accessibility, `static` modifier, `LightResults.Result` return
type, and single input parameter type must all match exactly.

```csharp
using LightObjects.Generated;
using LightResults;

namespace MyProject.Identifiers;

[GeneratedIdentifier<int>]
public readonly partial struct PositiveOrderId
{
    private static Result Validate(int value)
    {
        if (value <= 0)
            return Result.Failure("The value must be greater than zero.");

        return Result.Success();
    }
}
```

When the generator detects the exact `private static Result Validate(TValue value)` signature, it does
not emit its default validation method and the generated `Create`, `TryCreate`, `Parse`, and `TryParse`
methods call the custom method instead.

If the method does not match exactly, it is not treated as custom validation and the generator emits
the default validation method.

For string identifiers, custom validation replaces the default null, empty, and whitespace validation,
so include those checks yourself when they still matter.

### Accessing the underlying value

Generated numeric and `Guid` identifiers expose a typed conversion method.

```csharp
[GeneratedIdentifier<int>]
public readonly partial struct OrderId;
```

```csharp
var orderId = OrderId.Create(42);
var value = orderId.ToInt32();
```

All generated identifiers also implement `IValueObject<TValue, TSelf>`.

```csharp
var rawValue = ((IValueObject<Guid, CustomerId>)customerId).Value;
```

### JSON and type conversion

Generated identifiers include `System.Text.Json` converters. Non-generic identifiers and generic
class identifiers also support `TypeConverter` conversion. Generic struct identifiers intentionally
omit `TypeConverter` metadata because .NET does not provide a component-model attribute path that can
pass the closed generic struct type to the converter.

```csharp
using System.Text.Json;

public sealed record Customer
{
    public required CustomerId Id { get; init; }
    public required string Name { get; init; }
}
```

```csharp
var customer = new Customer
{
    Id = CustomerId.Create(Guid.NewGuid()),
    Name = "Ada",
};
var json = JsonSerializer.Serialize(customer);
var roundTripped = JsonSerializer.Deserialize<Customer>(json);
```

### Creating a manual value object

You can implement the interfaces directly when a value object needs custom behavior.

```csharp
using LightObjects;
using LightResults;

public readonly record struct EmailAddress :
    ICreatableValueObject<string, EmailAddress>,
    IValueObject<string, EmailAddress>
{
    public string Value { get; init; }

    public static EmailAddress Create(string value)
    {
        var result = TryCreate(value);
        if (result.IsSuccess(out var emailAddress, out var error))
            return emailAddress;

        throw new ValueObjectException(error.Message);
    }

    public static Result<EmailAddress> TryCreate(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !value.Contains('@', StringComparison.Ordinal))
            return Result.Failure<EmailAddress>("The email address is invalid.");

        return Result.Success(new EmailAddress { Value = value });
    }
}
```

### Detecting value object types

`TypeExtensions` can detect whether a type implements `IValueObject<TValue, TSelf>` and expose the
underlying value type.

```csharp
if (typeof(CustomerId).IsValueObjectType(out var valueType))
{
    Console.WriteLine(valueType.Name);
}
```

## What's new in v10.0

LightObjects 10.0 ships the runtime library and source generator in one NuGet package. Consumers only
need to reference `LightObjects`.

### Migrating from LightObjects.Generated

The old `LightObjects.Generated` NuGet package is deprecated and replaced by the single `LightObjects`
package.

1. Remove the `LightObjects.Generated` package reference.
2. Add or update the `LightObjects` package reference to version `10.0.0` or later.
3. Keep existing `using LightObjects.Generated;` statements. The generated attribute namespace has not changed.
