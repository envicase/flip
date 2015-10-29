using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace Flip.Tests
{
    using static It;
    using static Times;

    public class ConnectionExtensionsTest
    {
        [Theory AutoData]
        public async Task EmitRelaysWithModelInstance(User user)
        {
            var connection = Mock.Of<IConnection<User, string>>();
            IObservable<User> observable = null;
            Mock.Get(connection)
                .Setup(x => x.Emit(IsAny<IObservable<User>>()))
                .Callback<IObservable<User>>(p => observable = p);

            connection.Emit(user);

            Mock.Get(connection)
                .Verify(x => x.Emit(IsAny<IObservable<User>>()), Once());
            User actual = await observable;
            actual.Should().BeSameAs(user);
        }

        [Theory, AutoData]
        public async Task EmitRelaysWithFuture(User user)
        {
            var connection = Mock.Of<IConnection<User, string>>();
            IObservable<User> observable = null;
            Mock.Get(connection)
                .Setup(x => x.Emit(IsAny<IObservable<User>>()))
                .Callback<IObservable<User>>(p => observable = p);

            connection.Emit(Task.FromResult(user));

            Mock.Get(connection)
                .Verify(x => x.Emit(IsAny<IObservable<User>>()), Once());
            User actual = await observable;
            actual.Should().BeSameAs(user);
        }
    }
}
