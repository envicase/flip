using System;
using FluentAssertions;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace Flip.Tests
{
    public class OptionTest
    {
        [Fact]
        public void IsValueType() =>
            typeof(Option<>).IsValueType.Should().BeTrue();

        [Fact]
        public void ImplicitConverterConvertsNullToOption()
        {
            // Arrange

            // Act
            Option<string> option = null;

            // Assert
            option.HasValue.Should().BeFalse();
            option.ValueOrDefault.Should().BeNull();
        }

        [Theory, AutoData]
        public void ImplicitConverterConvertsValueToOption(string value)
        {
            // Arrange

            // Act
            Option<string> option = value;

            // Assert
            option.HasValue.Should().BeTrue();
            option.ValueOrDefault.Should().Be(value);
        }

        [Fact]
        public void CreateReturnsNoneWithNull()
        {
            // Arrange

            // Act
            Option<string> option = Option.Create<string>(null);

            // Assert
            option.HasValue.Should().BeFalse();
            option.ValueOrDefault.Should().BeNull();
        }

        [Theory, AutoData]
        public void CreateReturnsSomeWithValue(string value)
        {
            // Arrange

            // Act
            Option<string> option = Option.Create(value);

            // Assert
            option.HasValue.Should().BeTrue();
            option.ValueOrDefault.Should().Be(value);
        }

        private class Anonymous { }

        [Fact]
        public void ImplementsIEquatable()
        {
            var sut = default(Option<Anonymous>);
            sut.Should().BeAssignableTo<IEquatable<Option<Anonymous>>>();
        }

        [Fact]
        public void EqualsReturnsTrueWithNoneAndNone()
        {
            Option<string> x = null;
            Option<string> y = null;

            bool actual = x.Equals(y);

            actual.Should().BeTrue();
        }

        [Fact]
        public void EqualsReturnsFalseWithNoneAndSome()
        {
            Option<string> x = null;
            Option<string> y = "foo";

            bool actual = x.Equals(y);

            actual.Should().BeFalse();
        }

        [Fact]
        public void EqualsReturnsFalseWithSomeAndNone()
        {
            Option<string> x = "foo";
            Option<string> y = null;

            bool actual = x.Equals(y);

            actual.Should().BeFalse();
        }

        [Theory]
        [InlineData("foo", "foo", true)]
        [InlineData("foo", "bar", false)]
        public void EqualsReturnsEqualityOfValues(
            string xValue, string yValue, bool expected)
        {
            Option<string> x = xValue;
            Option<string> y = yValue;

            bool actual = x.Equals(y);

            actual.Should().Be(expected);
        }
    }
}
