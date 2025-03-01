namespace LightObjects;

public interface ICloneableValueObject<TSelf> : IValueObject<TSelf>
    where TSelf : notnull
{
    TSelf Clone();
}
