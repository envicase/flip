using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;

namespace Flip
{
    /// <summary>
    /// 반응형 모델 스트림을 제공합니다.
    /// </summary>
    /// <typeparam name="TModel">모델 형식입니다.</typeparam>
    /// <typeparam name="TId">모델 식별자 형식입니다.</typeparam>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "This class provides not streams of bytes but streams of model instances.")]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Streams should not be disposed outside the class.")]
    public static class Stream<TModel, TId>
        where TModel : class, IModel<TId>
        where TId : IEquatable<TId>
    {
        private sealed class Instance : IObserver<TModel>
        {
            private readonly TId _modelId;
            private readonly Subject<IObservable<TModel>> _spout;
            private readonly BehaviorSubject<TModel> _subject;

            public Instance(TId modelId)
            {
                _modelId = modelId;
                _spout = new Subject<IObservable<TModel>>();
                _subject = new BehaviorSubject<TModel>(value: null);

                _spout.Switch().Subscribe(this);
            }

            public TId ModelId => _modelId;

            public IObserver<IObservable<TModel>> Spout => _spout;

            void IObserver<TModel>.OnCompleted()
            {
                throw new NotSupportedException();
            }

            void IObserver<TModel>.OnError(Exception error)
            {
                throw new NotSupportedException();
            }

            [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object[])", Justification = "No argument to be formatted.")]
            void IObserver<TModel>.OnNext(TModel value)
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                if (value.Id == null)
                {
                    var message =
                        $"{nameof(value)}.{nameof(value.Id)} cannot be null.";
                    throw new ArgumentException(message, nameof(value));
                }
                if (value.Id.Equals(_modelId) == false)
                {
                    var message =
                        $"{nameof(value)}.{nameof(value.Id)}({value.Id})"
                        + $" is not equal to ({_modelId}).";
                    throw new ArgumentException(message, nameof(value));
                }

                IEqualityComparer<TModel> comparer = EqualityComparerSafe;

                if (comparer.Equals(value, _subject.Value))
                {
                    return;
                }

                TModel next = CoalesceWithLast(value);

                if (comparer.Equals(next, _subject.Value))
                {
                    return;
                }

                _subject.OnNext(next);
            }

            private TModel CoalesceWithLast(TModel model)
            {
                if (_subject.Value == null)
                {
                    return model;
                }

                TModel result = CoalescerSafe.Coalesce(model, _subject.Value);
                if (result.Id.Equals(_modelId) == false)
                {
                    throw InvalidCoalescingResultId;
                }
                return result;
            }

            [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object[])", Justification = "No argument to be formatted.")]
            private InvalidOperationException InvalidCoalescingResultId =>
                new InvalidOperationException($"The id of the coalescing result is not equal to ({_modelId}).");

            public IDisposable Subscribe(IObserver<TModel> observer) =>
                new Subscription(this, observer);

            /// <summary>
            /// 스트리림 구독 수명을 관리합니다.
            /// </summary>
            /// <remarks>
            /// <see cref="Subscribe(IObserver{TModel})"/> 메서드가
            /// <see cref="System.Reactive.Disposables.Disposable"/> 클래스 대신
            /// 이 클래스를 사용하는 이유는 힙 메모리를 사용하는 대리자 개체를
            /// 추가적으로 생성하지 않기 위해서입니다.
            /// </remarks>
            private class Subscription : IDisposable
            {
                private readonly Instance _stream;
                private IDisposable _inner;

                public Subscription(Instance stream, IObserver<TModel> observer)
                {
                    _stream = stream;
                    _inner = WeakSubscription.Create(stream._subject, observer);
                }

                public void Dispose()
                {
                    IDisposable subscription =
                        Interlocked.Exchange(ref _inner, null);
                    if (subscription == null)
                    {
                        return;
                    }
                    lock (_syncRoot)
                    {
                        subscription.Dispose();
                        if (false == _stream._subject.HasObservers)
                        {
                            RemoveUnsafe(_stream._modelId);
                        }
                    }
                }
            }

            public void Dispose()
            {
                _spout.Dispose();
                _subject.Dispose();
            }
        }

        private sealed class Connection : IConnection<TModel, TId>
        {
            private readonly Instance _stream;
            private readonly BehaviorSubject<TModel> _subject;
            private readonly IObservable<TModel> _observable;
            private readonly IDisposable _subscription;

            public Connection(Instance stream)
            {
                _stream = stream;
                _subject = new BehaviorSubject<TModel>(value: null);
                _observable = from m in _subject
                              where m != null
                              select m;
                _subscription = _stream.Subscribe(_subject);
            }

            ~Connection()
            {
                Dispose();
            }

            public TId ModelId => _stream.ModelId;

            public void Emit(IObservable<TModel> source)
            {
                if (source == null)
                {
                    throw new ArgumentNullException(nameof(source));
                }

                _stream.Spout.OnNext(source);
            }

            public IDisposable Subscribe(IObserver<TModel> observer)
            {
                if (observer == null)
                {
                    throw new ArgumentNullException(nameof(observer));
                }

                return _observable.Subscribe(observer);
            }

            public void Dispose()
            {
                _subscription.Dispose();
                GC.SuppressFinalize(this);
            }
        }

        private static readonly object _syncRoot = new object();

        private static readonly Dictionary<TId, Instance> _store =
            new Dictionary<TId, Instance>();

        /// <summary>
        /// 모델 인스턴스의 동질성 여부를 판단하는
        /// <see cref="IEqualityComparer{T}"/>를 가져오거나 설정합니다.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Class wide equality comparer should be provided.")]
        public static IEqualityComparer<TModel> EqualityComparer { get; set; }

        private static IEqualityComparer<TModel> EqualityComparerSafe =>
            EqualityComparer ?? EqualityComparer<TModel>.Default;

        /// <summary>
        /// 두 모델 인스턴스의 상태를 유착하는
        /// <see cref="ICoalescer{T}"/>를 가져오거나 설정합니다.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Class wide coalescer should be provided.")]
        public static ICoalescer<TModel> Coalescer { get; set; }

        private static ICoalescer<TModel> CoalescerSafe =>
            Coalescer ?? Coalescer<TModel>.Default;

        /// <summary>
        /// 지정한 모델 식별자에 대한 스트림이 존재하는지 검사합니다.
        /// </summary>
        /// <param name="modelId">모델 식별자입니다.</param>
        /// <returns>
        /// 식별자 <paramref name="modelId"/>에 대한 스트림이 존재하면
        /// <c>true</c>이고, 아니면 <c>false</c>입니다.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Class wide check function should be provided.")]
        public static bool ExistsFor(TId modelId)
        {
            lock (_syncRoot)
            {
                return _store.ContainsKey(modelId);
            }
        }

        /// <summary>
        /// 모든 스트림 인스턴스를 제거합니다.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Class wide reset function should be provided.")]
        public static void Clear()
        {
            lock (_syncRoot)
            {
                foreach (Instance stream in _store.Values)
                {
                    stream.Dispose();
                }
                _store.Clear();
            }
        }

        /// <summary>
        /// 지정한 모델 식별자에 대한 스트림에 연결합니다.
        /// </summary>
        /// <param name="modelId">모델 식별자입니다.</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Class wide connect function should be provided.")]
        public static IConnection<TModel, TId> Connect(TId modelId)
        {
            lock (_syncRoot)
            {
                Instance stream;
                if (false == _store.TryGetValue(modelId, out stream))
                {
                    _store.Add(modelId, stream = new Instance(modelId));
                }
                return new Connection(stream);
            }
        }

        private static void RemoveUnsafe(TId modelId)
        {
            Instance stream;
            if (false == _store.TryGetValue(modelId, out stream))
            {
                return;
            }
            stream.Dispose();
            _store.Remove(modelId);
        }
    }
}
