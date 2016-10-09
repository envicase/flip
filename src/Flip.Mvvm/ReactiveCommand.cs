﻿using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using static System.Reactive.Concurrency.Scheduler;

namespace Flip
{
    public static class ReactiveCommand
    {
        public static Func<IScheduler> SchedulerFactory { get; set; } =
            () => CurrentThread;

        private static IObservable<Func<object, bool>> CanAlwaysExecute =>
            Observable.Return<Func<object, bool>>(_ => true);

        private static Func<object, bool> ReturnTrue { get; } = _ => true;

        private static Func<object, bool> ReturnFalse { get; } = _ => false;

        public static ReactiveCommand<object> Create() =>
            new ReactiveCommand<object>(
                CanAlwaysExecute, p => Task.FromResult(p));

        public static ReactiveCommand<Unit> Create(Action<object> execute)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));

            return new ReactiveCommand<Unit>(
                CanAlwaysExecute,
                p =>
                {
                    execute.Invoke(p);
                    return Task.FromResult(Unit.Default);
                });
        }

        public static ReactiveCommand<Unit> Create(
            IObservable<bool> canExecuteSource, Action<object> execute)
        {
            if (canExecuteSource == null)
                throw new ArgumentNullException(nameof(canExecuteSource));

            if (execute == null)
                throw new ArgumentNullException(nameof(execute));

            return new ReactiveCommand<Unit>(
                canExecuteSource.Select(e => e ? ReturnTrue : ReturnFalse),
                p =>
                {
                    execute.Invoke(p);
                    return Task.FromResult(Unit.Default);
                });
        }

        public static ReactiveCommand<T> Create<T>(Func<object, T> execute)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));

            return new ReactiveCommand<T>(
                CanAlwaysExecute, p => Task.FromResult(execute.Invoke(p)));
        }

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

        public static ReactiveCommand<Unit> Create(Func<object, Task> execute)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));

            return new ReactiveCommand<Unit>(
                CanAlwaysExecute,
                async p =>
                {
                    await execute.Invoke(p);
                    return Unit.Default;
                });
        }

        public static ReactiveCommand<T> Create<T>(
            Func<object, Task<T>> execute)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));

            return new ReactiveCommand<T>(CanAlwaysExecute, execute);
        }

        public static ReactiveCommand<T> Create<T>(
            IObservable<bool> canExecuteSource,
            Func<object, Task<T>> execute)
        {
            if (canExecuteSource == null)
                throw new ArgumentNullException(nameof(canExecuteSource));

            if (execute == null)
                throw new ArgumentNullException(nameof(execute));

            return new ReactiveCommand<T>(
                canExecuteSource.Select(e => e ? ReturnTrue : ReturnFalse),
                execute);
        }

        public static ReactiveCommand<T> Create<T>(
            IObservable<Func<object, bool>> canExecuteSource,
            Func<object, Task<T>> execute)
        {
            if (canExecuteSource == null)
                throw new ArgumentNullException(nameof(canExecuteSource));

            if (execute == null)
                throw new ArgumentNullException(nameof(execute));

            return new ReactiveCommand<T>(canExecuteSource, execute);
        }
    };

    public class ReactiveCommand<T> : IReactiveCommand<T>
    {
        private Func<object, bool> _canExecute;
        private readonly Func<object, Task<T>> _execute;
        private readonly Subject<T> _spout;
        private readonly IScheduler _schedulerDelegate;

        public ReactiveCommand(
            IObservable<Func<object, bool>> canExecuteSource,
            Func<object, Task<T>> execute)
        {
            if (canExecuteSource == null)
                throw new ArgumentNullException(nameof(canExecuteSource));

            if (execute == null)
                throw new ArgumentNullException(nameof(execute));

            _execute = execute;
            _spout = new Subject<T>();
            _schedulerDelegate = new DelegatingScheduler(() => SchedulerSafe);

            canExecuteSource
                .ObserveOn(_schedulerDelegate)
                .Subscribe(OnNextCanExecute);
        }

        public IScheduler Scheduler { get; set; } =
            ReactiveCommand.SchedulerFactory?.Invoke() ?? CurrentThread;

        private IScheduler SchedulerSafe => Scheduler ?? Immediate;

        private void OnNextCanExecute(Func<object, bool> canExecute)
        {
            _canExecute = canExecute;
            RaiseCanExecuteChanged();
        }

        public event EventHandler CanExecuteChanged;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "'Raise' prefix is generally used to implement ICommand interface and to expose event fire function for PCLs.")]
        public void RaiseCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        public bool CanExecute(object parameter) =>
            _canExecute?.Invoke(parameter) ?? true;

        public async void Execute(object parameter) =>
            await ExecuteAsync(parameter);

        public async Task ExecuteAsync(object parameter)
        {
            try
            {
                if (CanExecute(parameter))
                    _spout.OnNext(await _execute.Invoke(parameter));
            }
            catch (Exception error)
            {
                _spout.OnError(error);
            }
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (observer == null)
                throw new ArgumentNullException(nameof(observer));

            return _spout.ObserveOn(_schedulerDelegate).Subscribe(observer);
        }

        protected virtual void Dispose(bool disposing) => _spout.Dispose();

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
