using System.Threading.Tasks;

namespace NullObject.UnitTests.TestObjects
{
    public interface IDerived : IBase
    {
        string String { get; set; }

        Task Test();
    }
}