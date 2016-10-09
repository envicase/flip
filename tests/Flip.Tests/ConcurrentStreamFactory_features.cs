using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Ploeh.AutoFixture;
using Xunit;

namespace Flip.Tests
{
    public class ConcurrentStreamFactory_features
    {
        public class Model : Model<int>
        {
            public Model(int id) : base(id)
            {
            }
        }

        private readonly IFixture _fixture = new Fixture();

        [Fact]
        public void sut_inherits_StreamFactoryBase()
        {
            var sut = typeof(ConcurrentStreamFactory<,>);
            sut.BaseType.GetGenericTypeDefinition().Should().Be(
                typeof(StreamFactoryBase<,>));
        }

        [Fact]
        public void sut_is_thread_safe()
        {
            // Arrange
            var sut = new ConcurrentStreamFactory<int, Model>();
            var generator = new Generator<int>(_fixture);
            int[] modelIds = generator.Take(10).ToArray();
            Dictionary<int, ConcurrentBag<IConnection<Model>>> connections =
                modelIds.ToDictionary(
                    modelId => modelId,
                    modelId => new ConcurrentBag<IConnection<Model>>());

            // Act
            Action action = () =>
            modelIds.SelectMany(modelId => Enumerable.Repeat(modelId, 10))
                    .OrderBy(modelId => generator.First())
                    .AsParallel()
                    .ForAll(modelId =>
                    {
                        if (generator.First() % 2 == 0)
                            connections[modelId].Add(sut.Connect(modelId));
                        else
                            connections[modelId].LastOrDefault()?.Dispose();
                    });

            // Assert
            action.ShouldNotThrow<Exception>();
        }
    }
}
