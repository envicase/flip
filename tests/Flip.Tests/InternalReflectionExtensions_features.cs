using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Xunit;

namespace Flip.Tests
{
    public class InternalReflectionExtensions_features
    {
        internal interface INonGeneric { }

        internal class Plain { }

        internal class NonGeneric : INonGeneric { }

        internal class SimpleGeneric<T> { }

        internal class MultipleGenerics<T1, T2, T3> { }

        internal class SpecifiedGeneric<T>
            where T : NonGeneric
        { }

        internal class NestedGenerics<T1, T2>
            where T1 : SimpleGeneric<T2>
        { }

        internal interface INestedComplexGenerics<T1, T2, T3>
            where T1 : NestedGenerics<T2, T3>
            where T2 : SimpleGeneric<T3>
            where T3 : NonGeneric
        { }

        internal class NestedComplexGenerics<T1, T2, T3>
            : INestedComplexGenerics<T1, T2, T3>
            where T1 : NestedGenerics<T2, T3>
            where T2 : SimpleGeneric<T3>
            where T3 : NonGeneric
        { }

        internal class BaseConstructors
        {
            protected BaseConstructors()
            {
            }

            protected BaseConstructors(int capacity)
            {
            }
        }

        internal class Constructors<T>
            : BaseConstructors
            where T : INestedComplexGenerics<NestedGenerics<SimpleGeneric<NonGeneric>, NonGeneric>, SimpleGeneric<NonGeneric>, NonGeneric>
        {
            protected Constructors()
            {
            }

            protected Constructors(int capacity) : base(capacity)
            {
            }

            public Constructors(
                int capacity,
                NonGeneric nonGeneric,
                SimpleGeneric<List<int>> simple,
                MultipleGenerics<int, string, Guid> multiple,
                T complex)
            { }
        }

        [Theory]
        [InlineData(typeof(INonGeneric), nameof(INonGeneric))]
        [InlineData(typeof(Plain), nameof(Plain))]
        [InlineData(typeof(NonGeneric), nameof(NonGeneric))]
        [InlineData(typeof(SimpleGeneric<string>), "SimpleGeneric<string>")]
        [InlineData(typeof(MultipleGenerics<int, string, DateTime>), "MultipleGenerics<Int32, String, DateTime>")]
        [InlineData(typeof(SpecifiedGeneric<NonGeneric>), "SpecifiedGeneric<NonGeneric>")]
        [InlineData(typeof(NestedGenerics<SimpleGeneric<int>, int>), "NestedGenerics<SimpleGeneric<Int32>, Int32>")]
        [InlineData(typeof(INestedComplexGenerics<NestedGenerics<SimpleGeneric<NonGeneric>, NonGeneric>, SimpleGeneric<NonGeneric>, NonGeneric>), "INestedComplexGenerics<NestedGenerics<SimpleGeneric<NonGeneric>, NonGeneric>, SimpleGeneric<NonGeneric>, NonGeneric>")]
        [InlineData(typeof(NestedComplexGenerics<NestedGenerics<SimpleGeneric<NonGeneric>, NonGeneric>, SimpleGeneric<NonGeneric>, NonGeneric>), "NestedComplexGenerics<NestedGenerics<SimpleGeneric<NonGeneric>, NonGeneric>, SimpleGeneric<NonGeneric>, NonGeneric>")]
        [InlineData(typeof(Dictionary<int, string>), "Dictionary<Int32, String>")]
        [InlineData(typeof(Dictionary<Int32, String>), "Dictionary<Int32, String>")]
        [InlineData(typeof(List<Nullable<int>>), "List<Nullable<Int32>>")]
        [InlineData(typeof(List<int?>), "List<Nullable<Int32>>")]
        public void GetFriendlyName_returns_friendly_type_name(Type type, string expected)
        {
            type.GetFriendlyName().Should().BeEquivalentTo(expected);
        }

        [Theory]
        [InlineData(typeof(NonGeneric), nameof(NonGeneric))]
        [InlineData(typeof(NestedComplexGenerics<NestedGenerics<SimpleGeneric<NonGeneric>, NonGeneric>, SimpleGeneric<NonGeneric>, NonGeneric>), "NestedComplexGenerics")]
        [InlineData(typeof(Dictionary<int, string>), "Dictionary")]
        [InlineData(typeof(Dictionary<Int32, String>), "Dictionary")]
        [InlineData(typeof(List<int?>), "List")]
        [InlineData(typeof(List<Nullable<Int32>>), "List")]
        public void GetConstructorName_returns_plain_type_name(Type type, string expected)
        {
            type.GetConstructorName().Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void GetConstructorName_throws_InvalidOperationException_for_interface()
        {
            Action action = () => typeof(INonGeneric).GetConstructorName();
            action.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void GetFriendlyName_works_correctly_for_simple_constructor()
        {
            var type = typeof(Constructors<NestedComplexGenerics<NestedGenerics<SimpleGeneric<NonGeneric>, NonGeneric>, SimpleGeneric<NonGeneric>, NonGeneric>>);
            var constructor = type
                .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                .OrderBy(c => c.GetParameters().Length)
                .First();
            constructor.GetFriendlyName()
                .Should()
                .BeEquivalentTo("Constructors()");
        }

        [Fact]
        public void GetFriendlyName_works_correctly_for_complex_constructor()
        {
            var type = typeof(Constructors<NestedComplexGenerics<NestedGenerics<SimpleGeneric<NonGeneric>, NonGeneric>, SimpleGeneric<NonGeneric>, NonGeneric>>);
            var constructor = type
                .GetConstructors()
                .OrderByDescending(c => c.GetParameters().Length)
                .First();
            var expected = "Constructors("
                + "Int32 capacity, "
                + "NonGeneric nonGeneric, "
                + "SimpleGeneric<List<Int32>> simple, "
                + "MultipleGenerics<Int32, String, Guid> multiple, "
                + "NestedComplexGenerics<NestedGenerics<SimpleGeneric<NonGeneric>, NonGeneric>, SimpleGeneric<NonGeneric>, NonGeneric> complex"
                + ")";
            constructor.GetFriendlyName().Should().BeEquivalentTo(expected);
        }
    }
}
