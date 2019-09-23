using System;

namespace NullObject.UnitTests.TestObjects
{
    public interface IMethodWithArgs
    {
        void Test(int @integer, long @long, bool @boolean, DateTime @dateTime, NullObjectUnitTests nullObjectUnitTests);
    }
}