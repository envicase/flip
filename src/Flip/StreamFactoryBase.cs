namespace Flip
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Runtime.CompilerServices;
    using static System.Environment;

    public abstract class StreamFactoryBase<TId, TModel> :
        IStreamFactory<TId, TModel>
        where TId : IEquatable<TId>
        where TModel : class, IModel<TId>
    {
        private readonly Dictionary<TId, Stream> _streams;
        private readonly IStreamFilter<TModel> _filter;

        internal StreamFactoryBase()
            : this(EmptyFilter.Instance)
        {
        }

        internal StreamFactoryBase(IStreamFilter<TModel> filter)
        {
            if (null == filter)
                throw new ArgumentNullException(nameof(filter));

            _filter = filter;
            _streams = new Dictionary<TId, Stream>();
        }

        public IStreamFilter<TModel> Filter => _filter;

        public IConnection<TId, TModel> Connect(TId modelId)
        {
            if (null == modelId)
                throw new ArgumentNullException(nameof(modelId));

            return new Connection(Invoke(() => GetOrAddStream(modelId)));
        }

        public bool ExistsFor(TId modelId) =>
            Invoke(() => _streams.ContainsKey(modelId));

        internal virtual T Invoke<T>(Func<T> action) => action.Invoke();

        private Stream GetOrAddStream(TId modelId)
        {
            Stream stream;
            if (false == _streams.TryGetValue(modelId, out stream))
            {
                stream = new Stream(this, modelId);
                _streams.Add(modelId, stream);
            }

            return stream;
        }

        private void OnStreamUnsubscribed(Stream stream)
        {
            bool removed = Invoke(() =>
                 stream.HasObservers
                 ? false
                 : _streams.Remove(stream.ModelId));
        }

        private sealed class Stream : ISubject<IObservable<TModel>, TModel>
        {
            private readonly StreamFactoryBase<TId, TModel> _factory;
            private readonly TId _modelId;
            private readonly Subject<IObservable<TModel>> _observer;
            private readonly BehaviorSubject<TModel> _observable;

            internal Stream(
                StreamFactoryBase<TId, TModel> factory,
                TId modelId)
            {
                _factory = factory;
                _modelId = modelId;
                _observer = new Subject<IObservable<TModel>>();
                _observable = new BehaviorSubject<TModel>(default(TModel));

                _observer.Switch()
                         .Select(Filter)
                         .Where(value => null != value)
                         .Subscribe(_observable);
            }

            internal TId ModelId => _modelId;

            internal bool HasObservers => _observable.HasObservers;

            internal TModel LastValue => _observable.Value;

            public void OnCompleted()
            {
                throw new NotSupportedException();
            }

            public void OnError(Exception error)
            {
                throw new NotSupportedException();
            }

            public void OnNext(IObservable<TModel> value)
            {
                _observer.OnNext(value);
            }

            public IDisposable Subscribe(IObserver<TModel> observer)
            {
                var subscription = _observable.Subscribe(observer);
                return Disposable.Create(() =>
                {
                    subscription.Dispose();
                    _factory.OnStreamUnsubscribed(this);
                });
            }

            private TModel Filter(TModel model)
            {
                if (null == model)
                    return null;

                ThrowIfModelHasInvalidId(model);

                IStreamFilter<TModel> filter = _factory.Filter;
                TModel filtered = filter.Execute(model, _observable.Value);
                if (null == filtered)
                    return null;

                ThrowIfModelHasInvalidId(filtered);

                return filtered;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void ThrowIfModelHasInvalidId(TModel model)
            {
                if (false == _modelId.Equals(model.Id))
                {
                    throw new InvalidOperationException(
                          "Model id is invalid."
                          + $"{NewLine} The expected value: {_modelId}"
                          + $"{NewLine} The actual value: {model.Id}");
                }
            }
        }

        private sealed class Connection : IConnection<TId, TModel>
        {
            private readonly Stream _stream;
            private readonly Subject<TModel> _observer;
            private readonly WeakSubscription<TModel> _subscription;

            public Connection(Stream stream)
            {
                if (null == stream)
                    throw new ArgumentNullException(nameof(stream));

                _stream = stream;
                _observer = new Subject<TModel>();
                _subscription = WeakSubscription.Create(_stream, _observer);
            }

            ~Connection()
            {
                _subscription.Dispose();
            }

            public TId ModelId => _stream.ModelId;

            public void Emit(IObservable<TModel> source)
            {
                if (null == source)
                    throw new ArgumentNullException(nameof(source));

                _stream.OnNext(source);
            }

            public IDisposable Subscribe(IObserver<TModel> observer)
            {
                if (null == observer)
                    throw new ArgumentNullException(nameof(observer));

                return _observer.Subscribe(observer);
            }

            public void Dispose()
            {
                _subscription.Dispose();
                GC.SuppressFinalize(this);
            }
        }

        private class EmptyFilter : IStreamFilter<TModel>
        {
            public static readonly EmptyFilter Instance = new EmptyFilter();

            public TModel Execute(TModel newValue, TModel lastValue)
                => newValue;
        }
    }
}
