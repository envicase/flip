namespace Flip
{
    using System;

    public interface IConnection<TModel> :
        IObservable<TModel>,
        IDisposable
        where TModel : class
    {
        void Emit(IObservable<TModel> source);
    }
}
