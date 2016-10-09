namespace Flip
{
    using System;
    using System.Linq;
    using FluentAssertions;
    using Moq;
    using Ploeh.AutoFixture;
    using Xunit;
    using Xunit.Abstractions;
    using static Moq.It;
    using static Moq.Times;

    public class Connection_features
    {
        public class Model : Model<int>
        {
            public Model(int id) : base(id)
            {
            }
        }

        private readonly IFixture _fixture = new Fixture();
        private readonly ITestOutputHelper _output;

        public Connection_features(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void sut_subscribes_only_models_having_specified_model_id()
        {
            // Arrange
            var factory = new StreamFactory<int, Model>();
            var idGenerator = new Generator<int>(_fixture);

            var modelId = idGenerator.First();
            IConnection<Model> sut = factory.Connect(modelId);
            var observer = Mock.Of<IObserver<Model>>();
            sut.Subscribe(observer);

            var otherModelId = idGenerator.First(x => x != modelId);
            IConnection<Model> otherConnection =
                               factory.Connect(otherModelId);

            // Act
            otherConnection.Emit(new Model(otherModelId));

            // Assert
            Mock.Get(observer).Verify(x => x.OnNext(IsAny<Model>()), Never());
        }

        [Fact]
        public void Subscribe_connects_observer_with_stream()
        {
            // Arrange
            var factory = new StreamFactory<int, Model>();
            var modelId = _fixture.Create<int>();
            IConnection<Model> sut = factory.Connect(modelId);
            var observer = Mock.Of<IObserver<Model>>();
            sut.Subscribe(observer);
            var revision = new Model(modelId);

            // Act
            factory.Connect(modelId).Emit(revision);

            // Assert
            Mock.Get(observer).Verify(x => x.OnNext(revision), Once());
        }

        [Fact]
        public void Subscribe_connects_observer_with_stream_weakly()
        {
            // Arrange
            var factory = new StreamFactory<int, Model>();
            var modelId = _fixture.Create<int>();
            var reference = new WeakReference(factory.Connect(modelId));
            var observer = Mock.Of<IObserver<Model>>();
            ((IConnection<Model>)reference.Target).Subscribe(observer);

            // Act
            GC.Collect();
            GC.WaitForPendingFinalizers();
            factory.Connect(modelId).Emit(new Model(modelId));

            // Assert
            reference.IsAlive.Should().BeFalse();
            Mock.Get(observer).Verify(x => x.OnNext(IsAny<Model>()), Never());
        }

        [Fact]
        public void sut_removes_unused_stream()
        {
            // Arrange
            var factory = new StreamFactory<int, Model>();
            var modelId = _fixture.Create<int>();
            factory.Connect(modelId);

            // Act
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // Assert
            factory.ExistsFor(modelId).Should().BeFalse();
        }
    }
}
