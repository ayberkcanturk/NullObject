using System;

using NullObject.UnitTests.TestObjects;

using Xunit;

namespace NullObject.UnitTests
{
    public class NullObjectUnitTests
    {
        [Fact]
        public void Should_BeAbleTo_Create_NullObjectOf_Empty()
        {
            var @interface = NullObject.Of<IEmpty>();
            Assert.True(@interface != null, "Object should not be null");
            Assert.True(@interface is IEmpty, "Wrong interface generated");
        }

        [Fact]
        public void Should_ThrowException_OtherThan_InterfaceTypes()
        {
            Assert.Throws<NotSupportedException>(() =>
            {
                var @interface = NullObject.Of<NullObjectUnitTests>();
            });
        }

        [Fact]
        public void Should_BeAbleTo_Create_NullObjectOf_PrimitiveDataType_WithDefaultValues()
        {
            var @interface = NullObject.Of<IPrimitiveDataType>();
            Assert.True(@interface != null, "Object should not be null");
            Assert.True(@interface is IPrimitiveDataType, "Wrong interface generated");

            Assert.True(@interface.Boolean == default(bool));
            Assert.True(@interface.DateTime == default(DateTime));
            Assert.True(@interface.Integer == default(int));
            Assert.True(@interface.String == default(string));
            Assert.True(@interface.Long == default(long));
        }

        [Fact]
        public void Should_BeAbleTo_Create_NullObjectOf_MethodWithoutArgs_With_Method_Test()
        {
            IMethodWithoutArgs testMethodWithoutArgs = NullObject.Of<IMethodWithoutArgs>();
            Assert.True(testMethodWithoutArgs is IMethodWithoutArgs, "Wrong interface generated.");
            testMethodWithoutArgs.Test();
        }

        [Fact]
        public void Should_BeAbleTo_Create_NullObjectOf_MethodWithArgsInterface_With_Method_Test()
        {
            IMethodWithArgs testMethodWithArgs = NullObject.Of<IMethodWithArgs>();
            Assert.True(testMethodWithArgs is IPropertyWithGetterOnly, "Wrong interface generated.");
            testMethodWithArgs.Test(default(int), default(long), default(bool), default(DateTime), new NullObjectUnitTests());
        }

        [Fact]
        public void Should_BeAbleTo_Create_NullObjectOf_PropertyWithGetterOnlyInterface()
        {
            IPropertyWithGetterOnly testPropertyWithGetterOnly = NullObject.Of<IPropertyWithGetterOnly>();
            Assert.True(testPropertyWithGetterOnly is IPropertyWithGetterOnly, "Wrong interface generated.");
            int @integer = testPropertyWithGetterOnly.Integer;
        }

        [Fact]
        public void Should_BeAbleTo_Create_NullObjectOf_PropertyWithSetterOnlyInterface()
        {
            IPropertyWithSetterOnly testPropertyWithSetterOnly = NullObject.Of<IPropertyWithSetterOnly>();
            Assert.True(testPropertyWithSetterOnly is IPropertyWithSetterOnly, "Wrong interface generated.");
            testPropertyWithSetterOnly.Integer = default(int);
        }

        [Fact]
        public void Should_BeAbleTo_Create_NullObjectOf_DerivedInterface_With_BaseMembers()
        {
            IDerived testDerivedInterface = NullObject.Of<IDerived>();
            Assert.True(testDerivedInterface is IDerived, "Wrong interface generated.");

            testDerivedInterface.String = "Test";
            testDerivedInterface.Integer = default(int);
            testDerivedInterface.Test();
        }

        [Fact]
        public void Should_BeAbleTo_Create_NullObjectOf_GenericValueTypeInterface()
        {
            IGenericValueTypeInterface<int> @interface = NullObject.Of<IGenericValueTypeInterface<int>>();
            Assert.Equal(default(int), @interface.ValueType);
        }

        [Fact]
        public void Should_BeAbleTo_Create_NullObjectOf_GenericReferenceTypeInterface()
        {
            IGenericReferenceType<NullObjectUnitTests> @interface = NullObject.Of<IGenericReferenceType<NullObjectUnitTests>>();
            var expected = new NullObjectUnitTests();
            @interface.ReferenceType = expected;
            Assert.Equal(expected, @interface.ReferenceType);
        }

        [Fact]
        public void Should_BeAbleTo_Create_NullObjectOf_Event()
        {
            IEvent @interface = NullObject.Of<IEvent>();
            Assert.True(@interface is IEvent, "Wrong interface generated.");
        }
    }
}