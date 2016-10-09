namespace Flip
{
    using System;
    using System.Threading.Tasks;
    using System.Windows.Input;

    public interface IReactiveCommand : ICommand, IDisposable
    {
        Task ExecuteAsync(object parameter);
    }

    public interface IReactiveCommand<T> : IReactiveCommand, IObservable<T>
    {
    }
}
