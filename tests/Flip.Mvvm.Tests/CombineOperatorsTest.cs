using System;
using FluentAssertions;
using Xunit;

namespace Flip.Tests
{
    using static CombineOperators;

    public class CombineOperatorsTest
    {
        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, false, true)]
        [InlineData(false, true, true)]
        [InlineData(false, false, false)]
        public void OrWith2ArgsReturnsCorrectValue(
            bool arg1, bool arg2, bool expected) =>
            Or(arg1, arg2).Should().Be(expected);

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, false, true)]
        [InlineData(false, true, true)]
        [InlineData(false, false, false)]
        public void OrWith3ArgsReturnsCorrectValue(
            bool repeat, bool last, bool expected) =>
            Or(repeat, repeat, last).Should().Be(expected);

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, false, true)]
        [InlineData(false, true, true)]
        [InlineData(false, false, false)]
        public void OrWith4ArgsReturnsCorrectValue(
            bool repeat, bool last, bool expected) =>
            Or(repeat, repeat, repeat, last).Should().Be(expected);

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, false, false)]
        public void AndWith2ArgsReturnsCorrectValue(
            bool arg1, bool arg2, bool expected) =>
            And(arg1, arg2).Should().Be(expected);

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, false, false)]
        public void AndWith3ArgsReturnsCorrectValue(
            bool repeat, bool last, bool expected) =>
            And(repeat, repeat, last).Should().Be(expected);

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, false, false)]
        public void AndWith4ArgsReturnsCorrectValue(
            bool repeat, bool last, bool expected) =>
            And(repeat, repeat, repeat, last).Should().Be(expected);
    }
}
