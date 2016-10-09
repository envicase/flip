namespace Flip
{
    using System;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using Xunit;
    using static Moq.It;

    public class ConnectionExtensions_features
    {
        public class Model
        {
        }

        [Fact]
        public async Task Emit_with_model_relays_correctly()
        {
            var connection = Mock.Of<IConnection<Model>>();
            var model = new Model();
            IObservable<Model> observable = null;
            Mock.Get(connection)
                .Setup(x => x.Emit(IsAny<IObservable<Model>>()))
                .Callback<IObservable<Model>>(p => observable = p);

            connection.Emit(model);

            Model actual = await observable;
            actual.Should().BeSameAs(model);
        }

        [Fact]
        public async Task Emit_with_future_relays_correctly()
        {
            var connection = Mock.Of<IConnection<Model>>();
            var model = new Model();
            IObservable<Model> observable = null;
            Mock.Get(connection)
                .Setup(x => x.Emit(IsAny<IObservable<Model>>()))
                .Callback<IObservable<Model>>(p => observable = p);

            connection.Emit(Task.FromResult(model));

            Model actual = await observable;
            actual.Should().BeSameAs(model);
        }
    }
}
