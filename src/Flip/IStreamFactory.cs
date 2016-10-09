namespace Flip
{
    using System;

    public interface IStreamFactory<TId, TModel>
        where TId : IEquatable<TId>
        where TModel : class, IModel<TId>
    {
        IConnection<TModel> Connect(TId modelId);
    }
}
