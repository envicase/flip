using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Flip
{
    using static Scheduler;

    /// <summary>
    /// <see cref="ReactiveCommand{T}"/> 제네릭 클래스를 지원하는
    /// 정적 멤버를 제공합니다.
    /// </summary>
    public static class ReactiveCommand
    {
        /// <summary>
        /// 명령 스케줄러 팩토리 메서드를 가져오거나 설정합니다.
        /// </summary>
        public static Func<IScheduler> SchedulerFactory { get; set; } =
            () => CurrentThread;

        private static IObservable<Func<object, bool>> CanAlwaysExecute =>
            Observable.Return<Func<object, bool>>(_ => true);

        private static Func<object, bool> ReturnTrue { get; } = _ => true;

        private static Func<object, bool> ReturnFalse { get; } = _ => false;

        /// <summary>
        /// <see cref="object"/> 형식에 대한 <see cref="ReactiveCommand{T}"/>
        /// 인스턴스를 생성합니다.
        /// </summary>
        /// <returns>생성된 명령입니다.</returns>
        public static ReactiveCommand<object> Create() =>
            new ReactiveCommand<object>(
                CanAlwaysExecute, p => Task.FromResult(p));

        /// <summary>
        /// <see cref="ICommand.Execute(object)"/> 구현을 사용하여
        /// <see cref="Unit"/> 형식에 대한 <see cref="ReactiveCommand{T}"/>
        /// 인스턴스를 생성합니다.
        /// </summary>
        /// <param name="execute">
        /// <see cref="ICommand.Execute(object)"/> 구현입니다.
        /// </param>
        /// <returns>생성된 명령입니다.</returns>
        public static ReactiveCommand<Unit> Create(Action<object> execute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            return new ReactiveCommand<Unit>(
                CanAlwaysExecute,
                p =>
                {
                    execute.Invoke(p);
                    return Task.FromResult(Unit.Default);
                });
        }

        /// <summary>
        /// <see cref="ICommand.CanExecute(object)"/> 반환값에 대한 관측가능한
        /// 시퀀스와 <see cref="ICommand.Execute(object)"/> 구현을 사용하여
        /// <see cref="Unit"/> 형식에 대한 <see cref="ReactiveCommand{T}"/>
        /// 인스턴스를 생성합니다.
        /// </summary>
        /// <param name="canExecuteSource">
        /// <see cref="ICommand.CanExecute(object)"/> 반환값을 제공하는
        /// 관측가능한 시퀀스입니다.
        /// </param>
        /// <param name="execute">
        /// <see cref="ICommand.Execute(object)"/> 구현입니다.
        /// </param>
        /// <returns>생성된 명령입니다.</returns>
        public static ReactiveCommand<Unit> Create(
            IObservable<bool> canExecuteSource, Action<object> execute)
        {
            if (canExecuteSource == null)
            {
                throw new ArgumentNullException(nameof(canExecuteSource));
            }
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            return new ReactiveCommand<Unit>(
                canExecuteSource.Select(e => e ? ReturnTrue : ReturnFalse),
                p =>
                {
                    execute.Invoke(p);
                    return Task.FromResult(Unit.Default);
                });
        }

        /// <summary>
        /// 값을 반환하는 <see cref="ICommand.Execute(object)"/> 구현을 사용하여
        /// <see cref="ReactiveCommand{T}"/> 인스턴스를 생성합니다.
        /// </summary>
        /// <typeparam name="T">명령 반환값 형식입니다.</typeparam>
        /// <param name="execute">
        /// <see cref="ICommand.Execute(object)"/> 구현입니다.
        /// </param>
        /// <returns>생성된 명령입니다.</returns>
        public static ReactiveCommand<T> Create<T>(Func<object, T> execute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            return new ReactiveCommand<T>(
                CanAlwaysExecute, p => Task.FromResult(execute.Invoke(p)));
        }

        /// <summary>
        /// <see cref="ICommand.CanExecute(object)"/> 구현과
        /// <see cref="ICommand.Execute(object)"/> 구현을 사용하여
        /// <see cref="ReactiveCommand{T}"/> 인스턴스를 생성합니다.
        /// </summary>
        /// <param name="canExecute">
        /// <see cref="ICommand.CanExecute(object)"/> 구현입니다.
        /// </param>
        /// <param name="execute">
        /// <see cref="ICommand.Execute(object)"/> 구현입니다.
        /// </param>
        /// <returns>생성된 명령입니다.</returns>
        public static ReactiveCommand<Unit> Create(
            Func<object, bool> canExecute, Action<object> execute)
        {
            return new ReactiveCommand<Unit>(
                Observable.Return(canExecute),
                p =>
                {
                    execute.Invoke(p);
                    return Task.FromResult(Unit.Default);
                });
        }

        /// <summary>
        /// 비동기 <see cref="ICommand.Execute(object)"/> 구현을 사용하여
        /// <see cref="Unit"/> 형식에 대한 <see cref="ReactiveCommand{T}"/>
        /// 인스턴스를 생성합니다.
        /// </summary>
        /// <param name="execute">
        /// 비동기 <see cref="ICommand.Execute(object)"/> 구현입니다.
        /// </param>
        /// <returns>생성된 명령입니다.</returns>
        public static ReactiveCommand<Unit> Create(Func<object, Task> execute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            return new ReactiveCommand<Unit>(
                CanAlwaysExecute,
                async p =>
                {
                    await execute.Invoke(p);
                    return Unit.Default;
                });
        }

        /// <summary>
        /// 값을 반환하는 비동기 <see cref="ICommand.Execute(object)"/> 구현을
        /// 사용하여 <see cref="ReactiveCommand{T}"/> 인스턴스를 생성합니다.
        /// </summary>
        /// <typeparam name="T">명령 반환값 형식입니다.</typeparam>
        /// <param name="execute">
        /// 비동기 <see cref="ICommand.Execute(object)"/> 구현입니다.
        /// </param>
        /// <returns>생성된 명령입니다.</returns>
        public static ReactiveCommand<T> Create<T>(
            Func<object, Task<T>> execute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            return new ReactiveCommand<T>(CanAlwaysExecute, execute);
        }

        /// <summary>
        /// <see cref="ICommand.CanExecute(object)"/> 반환값에 대한 관측가능한
        /// 시퀀스와 값을 반환하는 비동기 <see cref="ICommand.Execute(object)"/>
        /// 구현을 사용하여 <see cref="ReactiveCommand{T}"/> 인스턴스를 생성합니다.
        /// </summary>
        /// <typeparam name="T">명령 반환값 형식입니다.</typeparam>
        /// <param name="canExecuteSource">
        /// <see cref="ICommand.CanExecute(object)"/> 반환값을 제공하는
        /// 관측가능한 시퀀스입니다.
        /// </param>
        /// <param name="execute">
        /// 비동기 <see cref="ICommand.Execute(object)"/> 구현입니다.
        /// </param>
        /// <returns>생성된 명령입니다.</returns>
        public static ReactiveCommand<T> Create<T>(
            IObservable<bool> canExecuteSource,
            Func<object, Task<T>> execute)
        {
            if (canExecuteSource == null)
            {
                throw new ArgumentNullException(nameof(canExecuteSource));
            }
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            return new ReactiveCommand<T>(
                canExecuteSource.Select(e => e ? ReturnTrue : ReturnFalse),
                execute);
        }

        /// <summary>
        /// <see cref="ICommand.CanExecute(object)"/> 구현에 대한 관측가능한
        /// 시퀀스와 값을 반환하는 비동기 <see cref="ICommand.Execute(object)"/>
        /// 구현을 사용하여 <see cref="ReactiveCommand{T}"/> 인스턴스를 생성합니다.
        /// </summary>
        /// <typeparam name="T">명령 반환값 형식입니다.</typeparam>
        /// <param name="canExecuteSource">
        /// <see cref="ICommand.CanExecute(object)"/> 구현을 제공하는
        /// 관측가능한 시퀀스입니다.
        /// </param>
        /// <param name="execute">
        /// 비동기 <see cref="ICommand.Execute(object)"/> 구현입니다.
        /// </param>
        /// <returns>생성된 명령입니다.</returns>
        public static ReactiveCommand<T> Create<T>(
            IObservable<Func<object, bool>> canExecuteSource,
            Func<object, Task<T>> execute)
        {
            if (canExecuteSource == null)
            {
                throw new ArgumentNullException(nameof(canExecuteSource));
            }
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            return new ReactiveCommand<T>(canExecuteSource, execute);
        }
    };

    /// <summary>
    /// <see cref="IReactiveCommand{T}"/> 인터페이스 구현체를 제공합니다.
    /// </summary>
    /// <typeparam name="T">명령 실행 결과 형식입니다.</typeparam>
    public class ReactiveCommand<T> : IReactiveCommand<T>
    {
        private Func<object, bool> _canExecute;
        private readonly Func<object, Task<T>> _execute;
        private readonly Subject<T> _spout;
        private readonly IScheduler _schedulerDelegate;

        /// <summary>
        /// <see cref="ReactiveCommand{T}"/> 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="canExecuteSource">
        /// <see cref="CanExecute(object)"/> 메서드 구현체 소스입니다.
        /// </param>
        /// <param name="execute">
        /// <see cref="ExecuteAsync(object)"/> 메서드 구현체입니다.
        /// </param>
        public ReactiveCommand(
            IObservable<Func<object, bool>> canExecuteSource,
            Func<object, Task<T>> execute)
        {
            if (canExecuteSource == null)
            {
                throw new ArgumentNullException(nameof(canExecuteSource));
            }
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            _execute = execute;
            _spout = new Subject<T>();
            _schedulerDelegate = new DelegatingScheduler(() => SchedulerSafe);

            canExecuteSource
                .ObserveOn(_schedulerDelegate)
                .Subscribe(OnNextCanExecute);
        }

        /// <summary>
        /// 명령 실행 스케줄러를 가져오거나 설정합니다.
        /// </summary>
        public IScheduler Scheduler { get; set; } =
            ReactiveCommand.SchedulerFactory?.Invoke() ?? CurrentThread;

        private IScheduler SchedulerSafe => Scheduler ?? Immediate;

        private void OnNextCanExecute(Func<object, bool> canExecute)
        {
            _canExecute = canExecute;
            RaiseCanExecuteChanged();
        }

        /// <summary>
        /// 명령을 실행해야 하는지 여부에 영향을 주는 변경이 발생할 때 발생합니다.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// <see cref="CanExecuteChanged"/> 이벤트를 발생시킵니다.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "'Raise' prefix is generally used to implement ICommand interface and to expose event fire function for PCLs.")]
        public void RaiseCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// 명령을 현재 상태에서 실행할 수 있는지를 결정하는 메서드를 정의합니다.
        /// </summary>
        /// <param name="parameter">
        /// 명령에 사용된 데이터입니다. 명령에서 데이터를 전달할 필요가 없으면
        /// 이 개체를 <c>null</c>로 설정할 수 있습니다.
        /// </param>
        /// <returns>
        /// 이 명령을 실행할 수 있으면 <c>true</c>이고,
        /// 그렇지 않으면 <c>false</c>입니다.
        /// </returns>
        public bool CanExecute(object parameter) =>
            _canExecute?.Invoke(parameter) ?? true;

        /// <summary>
        /// 명령이 호출될 때 호출될 메서드를 정의합니다.
        /// </summary>
        /// <param name="parameter">
        /// 명령에 사용된 데이터입니다. 명령에서 데이터를 전달할 필요가 없으면
        /// 이 개체를 <c>null</c>로 설정할 수 있습니다.
        /// </param>
        public async void Execute(object parameter) =>
            await ExecuteAsync(parameter);

        /// <summary>
        /// 비동기 명령이 호출될 때 호출될 메서드를 정의합니다.
        /// </summary>
        /// <param name="parameter">
        /// 명령에 사용된 데이터입니다. 명령에서 데이터를 전달할 필요가 없으면
        /// 이 개체를 <c>null</c>로 설정할 수 있습니다.
        /// </param>
        /// <returns>비동기 작업입니다.</returns>
        public async Task ExecuteAsync(object parameter)
        {
            try
            {
                if (CanExecute(parameter))
                {
                    _spout.OnNext(await _execute.Invoke(parameter));
                }
            }
            catch (Exception error)
            {
                _spout.OnError(error);
            }
        }

        /// <summary>
        /// <see cref="IObserver{T}"/>가 알림을 받을 것임을 공급자에 알립니다.
        /// </summary>
        /// <param name="observer">알림을 받을 개체입니다.</param>
        /// <returns>
        /// 공급자가 알림 전송을 완료하기 전에 관찰자가 알림 수신을 중지할 수 있는
        /// 인터페이스에 대한 참조입니다.
        /// </returns>
        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (observer == null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            return _spout.ObserveOn(_schedulerDelegate).Subscribe(observer);
        }

        /// <summary>
        /// <see cref="ReactiveCommand{T}"/> 개체가 사용하는 관리되지 않는
        /// 리소스를 해제하고, 관리되는 리소스를 선택적으로 해제할 수 있습니다.
        /// </summary>
        /// <param name="disposing">
        /// 관리되는 리소스와 관리되지 않는 리소스를 모두 해제하려면 <c>true</c>로
        /// 설정하고, 관리되지 않는 리소스만 해제하려면 <c>false</c>로 설정합니다.
        /// </param>
        protected virtual void Dispose(bool disposing) => _spout.Dispose();

        /// <summary>
        /// <see cref="ReactiveCommand{T}"/> 개체가 사용하는
        /// 모든 리소스를 해제합니다.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
