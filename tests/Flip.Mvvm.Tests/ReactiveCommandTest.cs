using System;
using System.Reactive;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Windows.Input;
using FluentAssertions;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace Flip.Tests
{
    using static It;
    using static Times;

    public class ReactiveCommandTest
    {
        [Fact]
        public void ImplementsICommand() =>
            ReactiveCommand.Create().Should().BeAssignableTo<ICommand>();

        [Fact]
        public void RaiseCanExecuteChangedRaisesEvent()
        {
            var sut = ReactiveCommand.Create();
            sut.MonitorEvents();

            sut.RaiseCanExecuteChanged();

            sut.ShouldRaise(nameof(sut.CanExecuteChanged))
                .WithSender(sut).WithArgs<EventArgs>(a => a == EventArgs.Empty);
        }

        public class Create
        {
            [Theory, AutoData]
            public void InitializesWithNoParameter(object parameter)
            {
                var command = ReactiveCommand.Create();
                command.Should().NotBeNull();
                command.CanExecute(parameter).Should().BeTrue();
            }

            [Theory, AutoData]
            public void InitializesWithAsyncExecuteAction(object parameter)
            {
                var functor = Mock.Of<IFunctor>();

                var command = ReactiveCommand.Create(
                    p => functor.Func<object, Task>(p));
                command?.Execute(parameter);

                command.Should().NotBeNull();
                command.CanExecute(parameter).Should().BeTrue();
                Mock.Get(functor).Verify(f =>
                    f.Func<object, Task>(parameter), Once());
            }

            [Theory, AutoData]
            public void InitializesWithSyncExecuteAction(object parameter)
            {
                var functor = Mock.Of<IFunctor>();

                var command = ReactiveCommand.Create(p => functor.Action(p));
                command?.Execute(parameter);

                command.Should().NotBeNull();
                command.CanExecute(parameter).Should().BeTrue();
                Mock.Get(functor).Verify(f => f.Action(parameter), Once());
            }

            [Theory, AutoData]
            public void InitializesCanExecuteValueSourceAndSyncExecuteAction(
                object parameter)
            {
                var canExecuteSource = new BehaviorSubject<bool>(true);
                var functor = Mock.Of<IFunctor>();

                var command = ReactiveCommand.Create(
                    canExecuteSource, p => functor.Action(p));
                command?.Execute(parameter);
                canExecuteSource.OnNext(false);

                command.Should().NotBeNull();
                command.CanExecute(parameter).Should().BeFalse();
                Mock.Get(functor).Verify(f => f.Action(parameter), Once());
            }

            [Theory, AutoData]
            public void InitializesWithSyncExecuteFunc(object parameter)
            {
                var functor = Mock.Of<IFunctor>();

                var command = ReactiveCommand.Create(
                    p => functor.Func<object, object>(p));
                command?.Execute(parameter);

                command.Should().NotBeNull();
                command.CanExecute(parameter).Should().BeTrue();
                Mock.Get(functor).Verify(f =>
                    f.Func<object, object>(parameter), Once());
            }

            [Theory, AutoData]
            public void InitializesWithCanExecuteValueSource(
                Subject<bool> source, object parameter)
            {
                var functor = Mock.Of<IFunctor>();

                var command = ReactiveCommand.Create(
                    source, functor.Func<object, Task<Unit>>);
                command?.MonitorEvents();
                source.OnNext(false);

                command.Should().NotBeNull();
                command.ShouldRaise(nameof(command.CanExecuteChanged));
                command.CanExecute(parameter).Should().BeFalse();
            }

            [Theory, AutoData]
            public void InitializesWithCanExecuteFuncAndSyncExecuteAction(
                object parameter)
            {
                var functor = Mock.Of<IFunctor>(f =>
                    f.Func<object, bool>(parameter) == true);

                var command = ReactiveCommand.Create(
                    functor.Func<object, bool>, functor.Action);
                command?.Execute(parameter);

                command.Should().NotBeNull();
                Mock.Get(functor).Verify(f =>
                    f.Func<object, bool>(parameter), Once());
                Mock.Get(functor).Verify(f => f.Action(parameter), Once());
            }

            [Theory, AutoData]
            public void InitlaizesWithCanExecuteFunctionSource(
                Subject<Func<object, bool>> source, object parameter)
            {
                var sut = new ReactiveCommand<Unit>(
                    source, _ => Task.FromResult(Unit.Default));
                sut.MonitorEvents();

                source.OnNext(p => p == parameter);

                sut.ShouldRaise(nameof(sut.CanExecuteChanged)).WithSender(sut)
                    .WithArgs<EventArgs>(a => a == EventArgs.Empty);
                sut.CanExecute(parameter).Should().BeTrue();
                sut.CanExecute(new object()).Should().BeFalse();
            }
        }

        public class Execute
        {
            [Theory, AutoData]
            public async Task InvokesDelegate(object parameter)
            {
                var functor = Mock.Of<IFunctor>(f =>
                    f.Func<object, Task<Unit>>(parameter) ==
                        Task.FromResult(Unit.Default));
                var sut = ReactiveCommand.Create(
                    functor.Func<object, Task<Unit>>);

                await sut.ExecuteAsync(parameter);

                Mock.Get(functor).Verify(f =>
                    f.Func<object, Task<Unit>>(parameter), Once());
            }

            [Theory, AutoData]
            public async Task SendsUnit(object parameter)
            {
                var functor = Mock.Of<IFunctor>(f =>
                    f.Func<object, Task<Unit>>(parameter) ==
                        Task.FromResult(Unit.Default));
                var sut = ReactiveCommand.Create(
                    functor.Func<object, Task<Unit>>);
                var observer = Mock.Of<IObserver<Unit>>();
                sut.Subscribe(observer);

                await sut.ExecuteAsync(parameter);

                Mock.Get(observer).Verify(x => x.OnNext(IsAny<Unit>()), Once());
            }

            [Theory, AutoData]
            public async Task SendsThrownException(
                object parameter, InvalidOperationException error)
            {
                var functor = Mock.Of<IFunctor>();
                Mock.Get(functor)
                    .Setup(f => f.Func<object, Task<Unit>>(parameter))
                    .Throws(error);
                var sut = ReactiveCommand.Create(
                    functor.Func<object, Task<Unit>>);
                var observer = Mock.Of<IObserver<Unit>>();
                sut.Subscribe(observer);

                await sut.ExecuteAsync(parameter);

                Mock.Get(observer).Verify(x => x.OnError(error), Once());
            }
        }
    }
}
