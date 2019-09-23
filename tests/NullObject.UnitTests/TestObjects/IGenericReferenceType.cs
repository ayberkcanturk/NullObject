namespace NullObject.UnitTests.TestObjects
{
    public interface IGenericReferenceType<TReferenceType> where TReferenceType : new()
    {
        TReferenceType ReferenceType { get; set; }
    }
}