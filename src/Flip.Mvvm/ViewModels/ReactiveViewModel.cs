using System;
using System.ComponentModel;

namespace Flip.ViewModels
{
    using static ReactiveViewModel;

    /// <summary>
    /// <see cref="ReactiveViewModel{TModel, TId}"/> 제네릭 클래스를 지원하는
    /// 메서드 집합을 제공합니다.
    /// </summary>
    public static class ReactiveViewModel
    {
        /// <summary>
        /// 스트림 연결을 사용하여 <see cref="ReactiveViewModel{TModel, TId}"/>
        /// 클래스의 새 인스턴스를 초기화하고 반환합니다.
        /// </summary>
        /// <typeparam name="TModel">모델 형식입니다.</typeparam>
        /// <typeparam name="TId">모델 식별자 형식입니다.</typeparam>
        /// <param name="connection">스트림 연결입니다.</param>
        /// <returns>초기화된 인스턴스입니다.</returns>
        public static ReactiveViewModel<TModel, TId> With<TModel, TId>(
            IConnection<TModel, TId> connection)
            where TModel : class, IModel<TId>
            where TId : IEquatable<TId>
            => new ReactiveViewModel<TModel, TId>(connection);

        /// <summary>
        /// <see cref="ReactiveViewModel{TModel, TId}.Model"/> 속성에 대한
        /// 변경 이벤트 데이터를 가져옵니다.
        /// </summary>
        public static PropertyChangedEventArgs ModelChangedEventArgs { get; } =
            new PropertyChangedEventArgs("Model");
    }

    /// <summary>
    /// 반응형 모델을 관리하는 뷰모델을 제공합니다.
    /// </summary>
    /// <typeparam name="TModel">모델 형식입니다.</typeparam>
    /// <typeparam name="TId">모델 식별자 형식입니다.</typeparam>
    public class ReactiveViewModel<TModel, TId> : ObservableObject, IDisposable
        where TModel : class, IModel<TId>
        where TId : IEquatable<TId>
    {
        private static T NullArgumentGuard<T>(T value, string paramName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(paramName);
            }

            return value;
        }

        private readonly IConnection<TModel, TId> _connection;
        private TModel _model;

        /// <summary>
        /// 스트림 연결을 사용하여 <see cref="ReactiveViewModel{TModel, TId}"/>
        /// 클래스의 새 인스턴스를 초기화힙나다.
        /// </summary>
        /// <param name="connection">스트림 연결입니다.</param>
        public ReactiveViewModel(IConnection<TModel, TId> connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            _connection = connection;
            _connection.Subscribe(m => Model = m);
        }

        /// <summary>
        /// 모델 식별자를 사용하여 <see cref="ReactiveViewModel{TModel, TId}"/>
        /// 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="modelId">모델 식별자입니다.</param>
        public ReactiveViewModel(TId modelId)
            : this(Stream<TModel, TId>.Connect(
                NullArgumentGuard(modelId, nameof(modelId))))
        {
        }

        /// <summary>
        /// 모델 인스턴스를 사용하여 <see cref="ReactiveViewModel{TModel, TId}"/>
        /// 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="model">모델 인스턴스입니다.</param>
        public ReactiveViewModel(TModel model)
            : this(Stream<TModel, TId>.Connect(
                NullArgumentGuard(model, nameof(model)).Id))
        {
            _connection.Emit(model);
        }

        /// <summary>
        /// 모델 식별자를 가져옵니다.
        /// </summary>
        public TId ModelId => _connection.ModelId;

        /// <summary>
        /// 스트림 연결을 가져옵니다.
        /// </summary>
        public IConnection<TModel, TId> Connection => _connection;

        /// <summary>
        /// 모델 인스턴스를 가져옵니다.
        /// </summary>
        public TModel Model
        {
            get
            {
                return _model;
            }
            private set
            {
                if (false == Equals(_model, value))
                {
                    _model = value;
                    OnPropertyChanged(ModelChangedEventArgs);
                }
            }
        }

        /// <summary>
        /// <see cref="ReactiveViewModel{TModel, TId}"/> 개체가 사용하는
        /// 모든 리소스를 해제합니다.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// <see cref="ReactiveViewModel{TModel, TId}"/> 개체가 사용하는 관리되지
        /// 않는 리소스를 해제하고, 관리되는 리소스를 선택적으로 해제할 수 있습니다.
        /// </summary>
        /// <param name="disposing">
        /// 관리되는 리소스와 관리되지 않는 리소스를 모두 해제하려면 <c>true</c>로
        /// 설정하고, 관리되지 않는 리소스만 해제하려면 <c>false</c>로 설정합니다.
        /// </param>
        protected virtual void Dispose(bool disposing) => _connection.Dispose();
    }
}
