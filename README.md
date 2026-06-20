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

String identifiers validate that the value is not null, empty, or whitespace.

```csharp
using LightObjects.Generated;

namespace MyProject.Identifiers;

[GeneratedIdentifier<string>]
public sealed partial class ProductCode;
```

```csharp
var productCode = ProductCode.Create("ABC-123");
```

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

Generated identifiers include `System.Text.Json` converters and `TypeConverter` implementations.

```csharp
using System.Text.Json;

public sealed record Customer(CustomerId Id, string Name);
```

```csharp
var customer = new Customer(CustomerId.Create(Guid.NewGuid()), "Ada");
var json = JsonSerializer.Serialize(customer);
var roundTripped = JsonSerializer.Deserialize<Customer>(json);
```

### Creating a manual value object

You can implement the interfaces directly when a value object needs custom behavior.

```csharp
using LightObjects;
using LightResults;

public readonly record struct EmailAddress(string Value) :
    ICreatableValueObject<string, EmailAddress>,
    IValueObject<string, EmailAddress>
{
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

        return Result.Success(new EmailAddress(value));
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
