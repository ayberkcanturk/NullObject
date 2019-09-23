using System;

namespace NullObject.UnitTests.TestObjects
{
    public interface IEvent
    {
        event EventHandler Notify;
    }
}