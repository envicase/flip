using System;
using Flip.ViewModels;
using FluentAssertions;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace Flip.Tests.ViewModels
{
    using System.ComponentModel;
    using static It;
    using static Times;

    [Collection("using Stream<User, string>")]
    [ClearStreamAfterTest(typeof(User), typeof(string))]
    public class ReactiveViewModelTest
    {
        [Theory, AutoData]
        public void InitializesWithConnection(string id)
        {
            var connection =
                Mock.Of<IConnection<User, string>>(c => c.ModelId == id);

            var sut = new ReactiveViewModel<User, string>(connection);

            sut.ModelId.Should().Be(id);
            sut.Model.Should().BeNull();
            sut.Connection.Should().BeSameAs(connection);
        }

        [Theory, AutoData]
        public void InitializesWithModelId(string id)
        {
            var sut = new ReactiveViewModel<User, string>(id);

            sut.ModelId.Should().Be(id);
            sut.Connection.Should().NotBeNull();
            sut.Connection.ModelId.Should().Be(id);
            sut.Model.Should().BeNull();
        }

        [Theory, AutoData]
        public void InitializesWithModel(User model)
        {
            IConnection<User, string> connection =
                Stream<User, string>.Connect(model.Id);
            var functor = Mock.Of<IFunctor>();
            connection.Subscribe(functor.Action);

            var sut = new ReactiveViewModel<User, string>(model);

            sut.ModelId.Should().Be(model.Id);
            sut.Connection.Should().NotBeNull();
            sut.Connection.ModelId.Should().Be(model.Id);
            sut.Model.Should().BeSameAs(model);
            Mock.Get(functor).Verify(f => f.Action(model), Once());
        }

        [Theory, AutoData]
        public void WithInitializesWithConnection(string id)
        {
            var connection =
                Mock.Of<IConnection<User, string>>(c => c.ModelId == id);

            var sut = ReactiveViewModel.With(connection);

            sut.Should().NotBeNull();
            sut.ModelId.Should().Be(id);
            sut.Model.Should().BeNull();
            sut.Connection.Should().BeSameAs(connection);
        }

        [Theory, AutoData]
        public void SubscribesConnection(string id, string userName, string bio)
        {
            var user = new User(id, userName, bio);
            var connection =
                Mock.Of<IConnection<User, string>>(c => c.ModelId == id);
            IObserver<User> observer = null;
            Mock.Get(connection)
                .Setup(c => c.Subscribe(IsAny<IObserver<User>>()))
                .Callback<IObserver<User>>(p => observer = p);

            var sut = new ReactiveViewModel<User, string>(connection);
            observer?.OnNext(user);

            Mock.Get(connection)
                .Verify(c => c.Subscribe(IsAny<IObserver<User>>()), Once());
            sut.Model.Should().BeSameAs(user);
        }

        [Collection("using Stream<User, string>")]
        [ClearStreamAfterTest(typeof(User), typeof(string))]
        public class ModelSetter
        {
            [Fact]
            public void ModelSetterIsPrivate()
            {
                var sut = typeof(ReactiveViewModel<,>).GetProperty("Model");

                sut.Should().NotBeNull();
                sut.SetMethod.Should().NotBeNull();
                sut.SetMethod.IsPrivate.Should().BeTrue();
            }

            [Theory, AutoData]
            public void RaisesEventWithModelChangedEventArgs(User user)
            {
                var sut = new ReactiveViewModel<User, string>(user.Id);
                sut.MonitorEvents();

                Stream<User, string>.Connect(user.Id).Emit(user);

                sut.ShouldRaisePropertyChangeFor(x => x.Model)
                    .WithArgs<PropertyChangedEventArgs>(args => ReferenceEquals(
                        args, ReactiveViewModel.ModelChangedEventArgs));
            }
        }

        [Fact]
        public void DisposeDisposesConnection()
        {
            var connection = Mock.Of<IConnection<User, string>>();
            var sut = ReactiveViewModel.With(connection);

            sut.Dispose();

            Mock.Get(connection).Verify(c => c.Dispose(), Once());
        }
    }
}
