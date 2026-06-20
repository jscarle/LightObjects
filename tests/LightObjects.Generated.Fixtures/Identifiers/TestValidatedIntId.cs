using LightResults;

namespace LightObjects.Generated.Fixtures.Identifiers;

[GeneratedIdentifier<int>]
public partial struct TestValidatedIntId
{
    private static Result Validate(int value)
    {
        if (value <= 0)
            return Result.Failure("The value must be greater than zero.");

        return Result.Success();
    }
}
