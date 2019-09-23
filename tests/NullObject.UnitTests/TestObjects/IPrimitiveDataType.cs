using System;

namespace NullObject.UnitTests.TestObjects
{
    public interface IPrimitiveDataType
    {
        bool Boolean { get; set; }
        DateTime DateTime { get; set; }
        int Integer { get; set; }
        long Long { get; set; }
        string String { get; set; }
    }
}