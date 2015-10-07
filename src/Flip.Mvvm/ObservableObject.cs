using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Flip
{
    /// <summary>
    /// <see cref="INotifyPropertyChanged"/> 인터페이스 구현체를 제공합니다.
    /// </summary>
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        /// <summary>
        /// 속성 값이 변경될 때 발생합니다.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 개체의 속성이 변경되면 호출됩니다.
        /// </summary>
        /// <param name="e">이벤트 데이터입니다.</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e) =>
            PropertyChanged?.Invoke(this, e);

        /// <summary>
        /// 개체의 속성이 변경되면 호출됩니다.
        /// </summary>
        /// <param name="propertyName">변경된 속성 이름입니다.</param>
        protected void OnPropertyChanged(string propertyName) =>
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// 두 값이 동일한지 여부를 검사합니다.
        /// </summary>
        /// <typeparam name="T">비교할 값의 형식입니다.</typeparam>
        /// <param name="left">첫번째 값입니다.</param>
        /// <param name="right">두번째 값입니다.</param>
        /// <returns>
        /// <paramref name="left"/>와 <paramref name="right"/>가 동일하면
        /// <c>true</c>이고, 그렇지 않으면 <c>false</c>입니다.
        /// </returns>
        protected virtual bool Equals<T>(T left, T right) =>
            EqualityComparer<T>.Default.Equals(left, right);

        /// <summary>
        /// 속성에 새 값을 대입하고
        /// <see cref="PropertyChanged"/> 이벤트를 발생시킵니다.
        /// </summary>
        /// <typeparam name="T">변경될 속성의 형식입니다.</typeparam>
        /// <param name="store">속성 값이 저장되는 필드입니다.</param>
        /// <param name="value">변경 후 속성이 가지게될 값입니다.</param>
        /// <param name="propertyName">변경될 속성의 이름입니다.</param>
        /// <returns>
        /// <see cref="PropertyChanged"/> 이벤트가 발생되었으면 <c>true</c>이고,
        /// 그렇지 않으면 <c>false</c>입니다. 기존 값과 새 값이 동일하면
        /// 이벤트는 발생되지 않습니다.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#", Justification = "This form is generally used to implement observable properties")]
        protected bool SetValue<T>(
            ref T store, T value, [CallerMemberName]string propertyName = null)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }
            if (Equals(store, value))
            {
                return false;
            }

            store = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
