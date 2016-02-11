using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;
using static Flip.Stream<Flip.Tests.User, System.Guid>;
using static Moq.It;
using static Moq.Times;

namespace Flip.Tests
{
    [Collection(nameof(Stream<User, Guid>))]
    [ClearStreamAfterTest(typeof(User), typeof(Guid))]
    public class StreamTest
    {
        [Theory, AutoData]
        public void ExistsForReturnsFalseForUnconnected(Guid id) =>
            ExistsFor(id).Should().BeFalse();

        [Theory, AutoData]
        public void ExistsForReturnsTrueForConnected(Guid id)
        {
            IConnection<User, Guid> connection = Connect(id);
            bool actual = ExistsFor(id);
            actual.Should().BeTrue();
        }

        [Theory, AutoData]
        public void ClearRemovesAllStreams(List<Guid> ids)
        {
            List<IConnection<User, Guid>> connections =
                ids.Select(id => Connect(id)).ToList();

            Clear();

            ids.TrueForAll(id => false == ExistsFor(id)).Should().BeTrue();
        }

        [Theory, AutoData]
        public void ConnectReturnsConnection(Guid id)
        {
            // Arrange

            // Act
            IConnection<User, Guid> connection = Connect(id);

            // Assert
            connection.Should().NotBeNull();
            connection.ModelId.Should().Be(id);
        }

        [Theory, AutoData]
        public void ConnectionEmitPublishesModel(User user)
        {
            IConnection<User, Guid> connection = Connect(user.Id);
            var observer = Mock.Of<IObserver<User>>();
            connection.SubscribeOn(Scheduler.Immediate).Subscribe(observer);

            Connect(user.Id).Emit(user);

            Mock.Get(observer).Verify(x => x.OnNext(user), Once());
        }

        [Theory, AutoData]
        public void ConnectionSendsLastToNewObserver(User user)
        {
            IConnection<User, Guid> connection = Connect(user.Id);
            connection.Emit(user);
            var observer = Mock.Of<IObserver<User>>();

            connection.Subscribe(observer);

            Mock.Get(observer).Verify(x => x.OnNext(user), Once());
        }

        [Theory, AutoData]
        public void NewConnectionSendsLastToObserver(User user)
        {
            Connect(user.Id).Emit(user);
            IConnection<User, Guid> connection = Connect(user.Id);
            var observer = Mock.Of<IObserver<User>>();

            connection.Subscribe(observer);

            Mock.Get(observer).Verify(x => x.OnNext(user), Once());
        }

        [Theory, AutoData]
        public void DisposingConnectionUnsubscribes(User user)
        {
            IConnection<User, Guid> connection1 = Connect(user.Id);
            var observer = Mock.Of<IObserver<User>>();
            connection1.Subscribe(observer);
            IConnection<User, Guid> connection2 = Connect(user.Id);

            connection1.Dispose();
            connection2.Emit(user);

            Mock.Get(observer).Verify(x => x.OnNext(IsAny<User>()), Never());
        }

        [Theory, AutoData]
        public void RemovesStreamThatHasNoConnection(Guid id)
        {
            List<IConnection<User, Guid>> connections =
                Enumerable.Repeat(id, 10).Select(Connect).ToList();

            connections.ForEach(x => x.Dispose());

            ExistsFor(id).Should().BeFalse();
        }

        [Theory, AutoData]
        public void ConnectionIsReferencedWeakly(User user)
        {
            var functor = Mock.Of<IFunctor>();
            IObserver<User> observer = Observer.Create<User>(functor.Action);
            IConnection<User, Guid> connection = Connect(user.Id);
            Connect(user.Id).Subscribe(observer);

            GC.Collect();
            GC.WaitForFullGCComplete();
            connection.Emit(user);

            Mock.Get(functor).Verify(x => x.Action(IsAny<User>()), Never());
        }

        [Theory, AutoData]
        public void EmitInterceptsModelSameAsLast(User user)
        {
            IConnection<User, Guid> connection = Connect(user.Id);
            var observer = Mock.Of<IObserver<User>>();
            connection.Emit(user);
            connection.Subscribe(observer);
            Mock.Get(observer).Verify(x => x.OnNext(user), Once());

            connection.Emit(user);

            Mock.Get(observer).Verify(x => x.OnNext(user), Once());
        }

        [Theory, AutoData]
        public void EmitinterceptsModelEqualToLast(
            User user, string name, string bio, string email)
        {
            EqualityComparer = Mock.Of<IEqualityComparer<User>>(
                x => x.Equals(IsNotNull<User>(), IsNotNull<User>()) == true);
            IConnection<User, Guid> connection = Connect(user.Id);
            var observer = Mock.Of<IObserver<User>>();
            connection.Emit(user);
            connection.Subscribe(observer);
            Mock.Get(observer).Verify(x => x.OnNext(IsAny<User>()), Once());

            connection.Emit(new User(user.Id, name, bio, email));

            Mock.Get(observer).Verify(x => x.OnNext(IsAny<User>()), Once());
        }

        [Theory, AutoData]
        public async Task EmitInterceptsPreviouslyEmitted(
            User user, string name, string bio, string email)
        {
            IConnection<User, Guid> connection = Connect(user.Id);
            var observer = Mock.Of<IObserver<User>>();
            connection.Subscribe(observer);
            Task<User> task = Task.Delay(10).ContinueWith(_ => user);

            connection.Emit(task.ToObservable());
            connection.Emit(Observable.Return(
                new User(user.Id, name, bio, email)));
            await task;
            await Task.Delay(10);

            Mock.Get(observer).Verify(x => x.OnNext(user), Never());
            Mock.Get(observer).Verify(x => x.OnNext(
                Is<User>(p => p.UserName == name && p.Bio == bio)), Once());
        }
    }
}
