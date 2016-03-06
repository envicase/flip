using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;

namespace Flip.Tests
{
    public class InternalReflectionExtensionsTest
    {
        internal interface INonGeneric { }
        internal class Plain { }
        internal class NonGeneric : INonGeneric { }
        internal class SimpleGeneric<T> { }
        internal class MultipleGenerics<T1, T2, T3> { }
        internal class SpecifiedGeneric<T>
            where T : NonGeneric { }
        internal class NestedGenerics<T1, T2>
            where T1 : SimpleGeneric<T2> { }
        internal interface INestedComplexGenerics<T1, T2, T3>
            where T1 : NestedGenerics<T2, T3>
            where T2 : SimpleGeneric<T3>
            where T3 : NonGeneric { }
        internal class NestedComplexGenerics<T1, T2, T3>
            : INestedComplexGenerics<T1, T2, T3>
            where T1 : NestedGenerics<T2, T3>
            where T2 : SimpleGeneric<T3>
            where T3 : NonGeneric { }

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
        public void GetFriendlyNameShouldBeFriendly(Type type, string expected)
        {
            type.GetFriendlyName().Should().BeEquivalentTo(expected);
        }
    }
}
