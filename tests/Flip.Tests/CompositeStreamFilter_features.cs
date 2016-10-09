namespace Flip
{
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using Moq;
    using Ploeh.AutoFixture;
    using Xunit;
    using static Moq.It;
    using static Moq.Times;

    public class CompositeStreamFilter_features
    {
        private readonly IFixture _fixture = new Fixture();

        public class Model : Model<int>
        {
            public Model(int id) : base(id)
            {
            }
        }

        [Fact]
        public void sut_implements_IStreamFilter()
        {
            var sut = new CompositeStreamFilter<Model>();
            sut.Should().BeAssignableTo<IStreamFilter<Model>>();
        }

        [Fact]
        public void constructor_sets_filters_correctly()
        {
            // Arrange
            var filters = new List<IStreamFilter<Model>>(
                from _ in Enumerable.Range(0, 3)
                select Mock.Of<IStreamFilter<Model>>());

            // Act
            var sut = new CompositeStreamFilter<Model>(filters);
            IEnumerable<IStreamFilter<Model>> actual = sut.Filters;

            // Assert
            actual.ShouldAllBeEquivalentTo(
                   filters, opts => opts.WithStrictOrdering());
        }

        [Fact]
        public void Execute_returns_new_value_if_no_filter()
        {
            // Arrange
            var sut = new CompositeStreamFilter<Model>();
            var newValue = _fixture.Create<Model>();
            var lastValue = _fixture.Create<Model>();

            // Act
            Model actual = sut.Execute(newValue, lastValue);

            // Assert
            actual.Should().BeSameAs(newValue);
        }

        [Fact]
        public void Execute_relays_arguments_to_first_filter()
        {
            // Arrange
            var filter = Mock.Of<IStreamFilter<Model>>();
            var sut = new CompositeStreamFilter<Model>(filter);
            var newValue = _fixture.Create<Model>();
            var lastValue = _fixture.Create<Model>();

            // Act
            sut.Execute(newValue, lastValue);

            // Assert
            Mock.Get(filter).Verify(
                 x => x.Execute(newValue, lastValue), Once());
        }

        [Fact]
        public void Execute_does_not_execue_filter_if_previous_filter_returns_null()
        {
            // Arrange
            var newValue = _fixture.Create<Model>();
            var lastValue = _fixture.Create<Model>();
            Model nullModel = null;

            var firstFilter = Mock.Of<IStreamFilter<Model>>(
                x => x.Execute(newValue, lastValue) == nullModel);

            var sut = new CompositeStreamFilter<Model>(
                firstFilter, Mock.Of<IStreamFilter<Model>>());

            // Act
            sut.Execute(newValue, lastValue);

            // Assert
            Mock.Get(sut.Filters.Last()).Verify(
                 x => x.Execute(IsAny<Model>(), IsAny<Model>()), Never());
        }

        [Fact]
        public void Execute_uses_filter_result_as_new_value_argument_of_next_filter()
        {
            // Arrange
            var newValue = _fixture.Create<Model>();
            var lastValue = _fixture.Create<Model>();
            var filterResult = _fixture.Create<Model>();

            var firstFilter = Mock.Of<IStreamFilter<Model>>(
                x => x.Execute(newValue, lastValue) == filterResult);

            var sut = new CompositeStreamFilter<Model>(
                firstFilter, Mock.Of<IStreamFilter<Model>>());

            // Act
            sut.Execute(newValue, lastValue);

            // Assert
            Mock.Get(sut.Filters.Last()).Verify(
                 x => x.Execute(filterResult, lastValue), Once());
        }

        [Fact]
        public void Execute_returns_result_of_last_filter()
        {
            // Arrange
            var newValue = _fixture.Create<Model>();
            var lastValue = _fixture.Create<Model>();

            var firstResult = _fixture.Create<Model>();
            var firstFilter = Mock.Of<IStreamFilter<Model>>(
                x => x.Execute(newValue, lastValue) == firstResult);

            var secondResult = _fixture.Create<Model>();
            var secondFilter = Mock.Of<IStreamFilter<Model>>(
                x => x.Execute(firstResult, lastValue) == secondResult);

            var sut = new CompositeStreamFilter<Model>(
                firstFilter, secondFilter);

            // Act
            Model actual = sut.Execute(newValue, lastValue);

            // Assert
            actual.Should().BeSameAs(secondResult);
        }
    }
}
