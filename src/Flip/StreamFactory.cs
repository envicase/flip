namespace Flip
{
    using System;

    public class StreamFactory<TId, TModel> :
        StreamFactoryBase<TId, TModel>
        where TId : IEquatable<TId>
        where TModel : class, IModel<TId>
    {
        public StreamFactory()
            : base()
        {
        }

        public StreamFactory(IStreamFilter<TModel> filter)
            : base(filter)
        {
        }
    }
}
