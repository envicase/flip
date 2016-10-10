#pragma warning disable SA1402 // File may only contain a single class

namespace Flip.ViewModels
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ReactiveViewModel<TId, TModel> : ObservableObject, IDisposable
        where TId : IEquatable<TId>
        where TModel : class, IModel<TId>
    {
        private readonly IConnection<TId, TModel> _connection;
        private TModel _model;

        public ReactiveViewModel(
            Func<IConnection<TId, TModel>> connectionFactory)
        {
            if (connectionFactory == null)
                throw new ArgumentNullException(nameof(connectionFactory));

            IConnection<TId, TModel> connection = connectionFactory.Invoke();
            if (null == connection)
            {
                throw new InvalidOperationException(
                      $"{nameof(connectionFactory)} returned null reference.");
            }

            _connection = connection;
            _connection.Subscribe(m => Model = m);
        }

        ~ReactiveViewModel()
        {
            Dispose(disposing: false);
        }

        public TId ModelId => _connection.ModelId;

        public TModel Model
        {
            get
            {
                return _model;
            }

            private set
            {
                ThrowIfModelHasInvalidId(value);

                if (false == Equals(_model, value))
                {
                    _model = value;
                    OnPropertyChanged(ReactiveViewModel.ModelChangedEventArgs);
                }
            }
        }

        protected IConnection<TModel> Connection => _connection;

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) => _connection.Dispose();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThrowIfModelHasInvalidId(TModel model)
        {
            if (false == ModelId.Equals(model.Id))
            {
                throw new InvalidOperationException(
                      "Model id is invalid."
                      + $"{Environment.NewLine} The expected value: {ModelId}"
                      + $"{Environment.NewLine} The actual value: {model.Id}");
            }
        }
    }

    internal static class ReactiveViewModel
    {
        public static PropertyChangedEventArgs ModelChangedEventArgs { get; } =
               new PropertyChangedEventArgs("Model");
    }
}

#pragma warning restore SA1402 // File may only contain a single class
