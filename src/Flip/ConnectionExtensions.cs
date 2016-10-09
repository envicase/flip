using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace Flip
{
    public static class ConnectionExtensions
    {
        public static void Emit<TModel>(
            this IConnection<TModel> connection,
            TModel model)
            where TModel : class
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            if (model == null)
                throw new ArgumentNullException(nameof(model));

            connection.Emit(Observable.Return(model));
        }

        public static void Emit<TModel>(
            this IConnection<TModel> connection,
            Task<TModel> future)
            where TModel : class
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            if (future == null)
                throw new ArgumentNullException(nameof(future));

            connection.Emit(future.ToObservable());
        }
    }
}
