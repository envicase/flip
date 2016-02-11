using System;
using System.ComponentModel;
using Flip.ViewModels;
using FluentAssertions;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;
using static Moq.It;
using static Moq.Times;

namespace Flip.Tests.ViewModels
{
    [Collection(nameof(Stream<User, Guid>))]
    [ClearStreamAfterTest(typeof(User), typeof(Guid))]
    public class ReactiveViewModelTest
    {
        [Theory, AutoData]
        public void InitializesWithConnection(Guid id)
        {
            var connection =
                Mock.Of<IConnection<User, Guid>>(c => c.ModelId == id);

            var sut = new ReactiveViewModel<User, Guid>(connection);

            sut.ModelId.Should().Be(id);
            sut.Model.Should().BeNull();
            sut.Connection.Should().BeSameAs(connection);
        }

        [Theory, AutoData]
        public void InitializesWithModelId(Guid id)
        {
            var sut = new ReactiveViewModel<User, Guid>(id);

            sut.ModelId.Should().Be(id);
            sut.Connection.Should().NotBeNull();
            sut.Connection.ModelId.Should().Be(id);
            sut.Model.Should().BeNull();
        }

        [Theory, AutoData]
        public void InitializesWithModel(User model)
        {
            IConnection<User, Guid> connection =
                Stream<User, Guid>.Connect(model.Id);
            var functor = Mock.Of<IFunctor>();
            connection.Subscribe(functor.Action);

            var sut = new ReactiveViewModel<User, Guid>(model);

            sut.ModelId.Should().Be(model.Id);
            sut.Connection.Should().NotBeNull();
            sut.Connection.ModelId.Should().Be(model.Id);
            sut.Model.Should().BeSameAs(model);
            Mock.Get(functor).Verify(f => f.Action(model), Once());
        }

        [Theory, AutoData]
        public void WithInitializesWithConnection(Guid id)
        {
            var connection =
                Mock.Of<IConnection<User, Guid>>(c => c.ModelId == id);

            var sut = ReactiveViewModel.With(connection);

            sut.Should().NotBeNull();
            sut.ModelId.Should().Be(id);
            sut.Model.Should().BeNull();
            sut.Connection.Should().BeSameAs(connection);
        }

        [Theory, AutoData]
        public void SubscribesConnection(
            Guid id, string userName, string bio, string email)
        {
            var user = new User(id, userName, bio, email);
            var connection =
                Mock.Of<IConnection<User, Guid>>(c => c.ModelId == id);
            IObserver<User> observer = null;
            Mock.Get(connection)
                .Setup(c => c.Subscribe(IsAny<IObserver<User>>()))
                .Callback<IObserver<User>>(p => observer = p);

            var sut = new ReactiveViewModel<User, Guid>(connection);
            observer?.OnNext(user);

            Mock.Get(connection)
                .Verify(c => c.Subscribe(IsAny<IObserver<User>>()), Once());
            sut.Model.Should().BeSameAs(user);
        }

        [Collection(nameof(Stream<User, Guid>))]
        [ClearStreamAfterTest(typeof(User), typeof(Guid))]
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
                var sut = new ReactiveViewModel<User, Guid>(user.Id);
                sut.MonitorEvents();

                Stream<User, Guid>.Connect(user.Id).Emit(user);

                sut.ShouldRaisePropertyChangeFor(x => x.Model)
                    .WithArgs<PropertyChangedEventArgs>(args => ReferenceEquals(
                        args, ReactiveViewModel.ModelChangedEventArgs));
            }
        }

        [Fact]
        public void DisposeDisposesConnection()
        {
            var connection = Mock.Of<IConnection<User, Guid>>();
            var sut = ReactiveViewModel.With(connection);

            sut.Dispose();

            Mock.Get(connection).Verify(c => c.Dispose(), Once());
        }
    }
}
