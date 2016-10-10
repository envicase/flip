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

    public interface IConnection<TId, TModel> :
        IConnection<TModel>,
        IObservable<TModel>,
        IDisposable
        where TId : IEquatable<TId>
        where TModel : class, IModel<TId>
    {
        TId ModelId { get; }
    }
}
