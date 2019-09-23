namespace NullObject.UnitTests.TestObjects
{
    public interface IGenericValueTypeInterface<TValueType>
    {
        TValueType ValueType { get; set; }
    }
}