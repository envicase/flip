using System;

namespace Flip
{
    public class ConcurrentStreamFactory<TId, TModel> :
        StreamFactoryBase<TId, TModel>
        where TId : IEquatable<TId>
        where TModel : class, IModel<TId>
    {
        private readonly object _syncRoot = new object();

        public ConcurrentStreamFactory()
            : base()
        {
        }

        public ConcurrentStreamFactory(IStreamFilter<TModel> filter)
            : base(filter)
        {
        }

        internal override T Invoke<T>(Func<T> action)
        {
            lock (_syncRoot)
            {
                return base.Invoke(action);
            }
        }
    }
}
