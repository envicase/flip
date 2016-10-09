namespace Flip
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using FluentAssertions;
    using Moq;
    using Ploeh.AutoFixture;
    using Xunit;
    using static Moq.It;
    using static Moq.Times;

    public class StreamFactory_features
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
            var sut = typeof(StreamFactory<,>);
            sut.BaseType.GetGenericTypeDefinition().Should().Be(
                typeof(StreamFactoryBase<,>));
        }

        [Fact]
        public void constructor_sets_Filter_correctly()
        {
            var filter = Mock.Of<IStreamFilter<Model>>();
            var sut = new StreamFactory<int, Model>(filter);
            sut.Filter.Should().BeSameAs(filter);
        }

        [Fact]
        public void Connect_returns_connection_instance()
        {
            var sut = new StreamFactory<int, Model>();
            var modelId = _fixture.Create<int>();

            IConnection<Model> actual = sut.Connect(modelId);

            actual.Should().NotBeNull();
        }

        [Fact]
        public void Connect_returns_new_instances_for_every_call()
        {
            var sut = new StreamFactory<int, Model>();
            var modelId = _fixture.Create<int>();

            var actual = new List<IConnection<Model>>(
                from _ in Enumerable.Range(0, 3)
                select sut.Connect(modelId));

            actual.Should().OnlyContain(x => x != null);
            actual.ShouldAllBeEquivalentTo(actual.Distinct());
        }

        [Fact]
        public void Connect_returns_connection_instances_connected_to_each_other_for_every_call()
        {
            // Arrange
            var sut = new StreamFactory<int, Model>();
            var modelId = _fixture.Create<int>();

            // Act
            var actual = new List<IConnection<Model>>(
                from _ in Enumerable.Range(0, 3)
                select sut.Connect(modelId));

            // Assert
            foreach (IConnection<Model> source in actual)
            {
                var observers = new List<IObserver<Model>>();
                var subscriptions = new List<IDisposable>();
                foreach (IConnection<Model> target in actual)
                {
                    var observer = Mock.Of<IObserver<Model>>();
                    IDisposable subscription = target.Subscribe(observer);
                    observers.Add(observer);
                    subscriptions.Add(subscription);
                }
                var revision = new Model(modelId);
                source.Emit(revision);
                foreach (IObserver<Model> observer in observers)
                    Mock.Get(observer).Verify(x => x.OnNext(revision), Once());
            }
        }

        [Fact]
        public void ExistsFor_returns_false_for_nonexistent_model_id()
        {
            var sut = new StreamFactory<int, Model>();
            var modelId = _fixture.Create<int>();

            bool actual = sut.ExistsFor(modelId);

            actual.Should().BeFalse();
        }

        [Fact]
        public void ExistsFor_returns_true_for_model_id_with_which_stream_created()
        {
            var sut = new StreamFactory<int, Model>();
            var modelId = _fixture.Create<int>();
            IConnection<Model> connection = sut.Connect(modelId);

            bool actual = sut.ExistsFor(modelId);

            actual.Should().BeTrue();
        }

        [Fact]
        public void Stream_sends_new_revision_to_filter()
        {
            // Arrange
            int modelId = _fixture.Create<int>();
            var revision = new Model(modelId);
            var factory = new StreamFactory<int, Model>(
                Mock.Of<IStreamFilter<Model>>());
            IConnection<Model> connection = factory.Connect(modelId);

            // Act
            connection.Emit(revision);

            // Assert
            Mock.Get(factory.Filter).Verify(
                 x => x.Execute(revision, null), Once());
        }

        [Fact]
        public void Stream_sends_last_revision_to_filter()
        {
            // Arrange
            int modelId = _fixture.Create<int>();

            var firstRevision = new Model(modelId);
            var secondRevision = new Model(modelId);

            var factory = new StreamFactory<int, Model>(
                Mock.Of<IStreamFilter<Model>>());
            Mock.Get(factory.Filter)
                .Setup(x => x.Execute(firstRevision, null))
                .Returns(firstRevision);

            IConnection<Model> connection = factory.Connect(modelId);
            connection.Emit(firstRevision);

            // Act
            connection.Emit(secondRevision);

            // Assert
            Mock.Get(factory.Filter).Verify(
                 x => x.Execute(secondRevision, firstRevision), Once());
        }

        [Fact]
        public void Stream_sends_filter_result_to_connections()
        {
            // Arrange
            int modelId = _fixture.Create<int>();

            var revision = new Model(modelId);
            var filtered = new Model(modelId);

            var factory = new StreamFactory<int, Model>(
                Mock.Of<IStreamFilter<Model>>(x =>
                x.Execute(revision, null) == filtered));

            IConnection<Model> connection = factory.Connect(modelId);
            var observer = Mock.Of<IObserver<Model>>();
            connection.Subscribe(observer);

            // Act
            connection.Emit(revision);

            // Assert
            Mock.Get(observer).Verify(x => x.OnNext(filtered), Once());
        }

        [Fact]
        public void Stream_does_not_send_value_to_connections_if_filter_returns_null()
        {
            // Arrange
            int modelId = _fixture.Create<int>();

            var revision = new Model(modelId);
            Model filtered = null;

            var factory = new StreamFactory<int, Model>(
                Mock.Of<IStreamFilter<Model>>(x =>
                x.Execute(revision, null) == filtered));

            IConnection<Model> connection = factory.Connect(modelId);
            var observer = Mock.Of<IObserver<Model>>();
            connection.Subscribe(observer);

            // Act
            connection.Emit(revision);

            // Assert
            Mock.Get(observer).Verify(x => x.OnNext(IsAny<Model>()), Never());
        }

        [Fact]
        public void Stream_does_not_emit_null_value()
        {
            // Arrange
            int modelId = _fixture.Create<int>();

            var factory = new StreamFactory<int, Model>(
                Mock.Of<IStreamFilter<Model>>());

            IConnection<Model> connection = factory.Connect(modelId);
            var observer = Mock.Of<IObserver<Model>>();
            connection.Subscribe(observer);

            // Act
            connection.Emit(Observable.Return(default(Model)));

            // Assert
            Mock.Get(observer).Verify(
                 x => x.OnNext(IsAny<Model>()),
                 Never());
            Mock.Get(observer).Verify(
                 x => x.OnError(IsAny<Exception>()),
                 Never());
            Mock.Get(factory.Filter).Verify(
                 x => x.Execute(IsAny<Model>(), IsAny<Model>()),
                 Never());
        }

        [Fact]
        public void Stream_fails_if_model_id_invalid()
        {
            // Arrange
            var idGenerator = new Generator<int>(_fixture);
            int modelId = idGenerator.First();
            int invalidModelId = idGenerator.First(x => x != modelId);

            var factory = new StreamFactory<int, Model>();

            IConnection<Model> connection = factory.Connect(modelId);
            var observer = Mock.Of<IObserver<Model>>();
            connection.Subscribe(observer);

            // Act
            connection.Emit(new Model(invalidModelId));

            // Assert
            Mock.Get(observer).Verify(
                 x => x.OnError(IsAny<InvalidOperationException>()), Once());
        }

        [Fact]
        public void Stream_fails_if_filter_result_id_invalid()
        {
            // Arrange
            var idGenerator = new Generator<int>(_fixture);
            int modelId = idGenerator.First();
            int invalidModelId = idGenerator.First(x => x != modelId);
            var filtered = new Model(invalidModelId);

            var factory = new StreamFactory<int, Model>(
                Mock.Of<IStreamFilter<Model>>(x =>
                x.Execute(IsAny<Model>(), IsAny<Model>()) == filtered));

            IConnection<Model> connection = factory.Connect(modelId);
            var observer = Mock.Of<IObserver<Model>>();
            connection.Subscribe(observer);

            // Act
            connection.Emit(new Model(modelId));

            // Assert
            Mock.Get(observer).Verify(
                 x => x.OnError(IsAny<InvalidOperationException>()), Once());
        }
    }
}
