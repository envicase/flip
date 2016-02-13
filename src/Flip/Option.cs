using System;
using System.Collections.Generic;

namespace Flip
{
    /// <summary>
    /// <see cref="Option{T}"/> 구조체를 지원하는 메서드를 지원합니다.
    /// </summary>
    public static class Option
    {
        /// <summary>
        /// <typeparamref name="T"/> 형식 값을 사용하여
        /// <see cref="Option{T}"/> 인스턴스를 생성합니다.
        /// </summary>
        /// <typeparam name="T">선택 형식의 대상 형식입니다.</typeparam>
        /// <param name="value">
        /// <see cref="Option{T}"/> 인스턴스를 만들 값입니다.
        /// </param>
        /// <returns>
        /// <paramref name="value"/>가 <c>null</c>이면 <see cref="Option{T}"/>의
        /// 기본값, 아니면 <paramref name="value"/>가 들어있는
        /// <see cref="Option{T}"/> 인스턴스입니다.
        /// </returns>
        public static Option<T> Create<T>(T value) =>
            value == null ? default(Option<T>) : new Option<T>(value);
    }

    /// <summary>
    /// <a href="https://en.wikipedia.org/wiki/Option_type">
    /// 선택 형식(Option type)</a>을 제공합니다.
    /// </summary>
    /// <typeparam name="T">대상 형식입니다.</typeparam>
    public struct Option<T> : IEquatable<Option<T>>
    {
        private readonly bool _hasValue;
        private readonly T _value;

        /// <summary>
        /// <typeparamref name="T"/> 형식 값을 사용하여 <see cref="Option{T}"/>
        /// 구조체의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="value">대상 값입니다.</param>
        public Option(T value)
        {
            _hasValue = value != null;
            _value = value;
        }

        /// <summary>
        /// <see cref="Option{T}"/>이 값을 가지는지 여부를 반환합니다.
        /// </summary>
        public bool HasValue => _hasValue;

        /// <summary>
        /// <see cref="Option{T}"/>이 값을 가지면 해당 값,
        /// 아니면 <typeparamref name="T"/> 형식의 기본값을 반환합니다.
        /// </summary>
        public T ValueOrDefault => _hasValue ? _value : default(T);

        /// <summary>
        /// <typeparamref name="T"/> 형식 값을 <see cref="Option{T}"/>으로
        /// 변환합니다.
        /// </summary>
        /// <param name="value">
        /// <see cref="Option{T}"/>으로 변환할 값입니다.
        /// </param>
        public static implicit operator Option<T>(T value)
        {
            return Option.Create(value);
        }

        /// <summary>
        /// 이 <see cref="Option{T}"/>과 다른 <see cref="Option{T}"/>이 동일한지
        /// 여부를 반환합니다.
        /// </summary>
        /// <param name="other">비교할 <see cref="Option{T}"/>입니다.</param>
        /// <returns>
        /// 이 <see cref="Option{T}"/>과 <paramref name="other"/>가 동일하면
        /// <c>true</c>, 아니면 <c>false</c>입니다.
        /// </returns>
        public bool Equals(Option<T> other)
        {
            return _hasValue == other._hasValue
                ? _hasValue
                    ? EqualityComparer<T>.Default.Equals(_value, other._value)
                    : true
                : false;
        }

        /// <summary>
        /// 해시 함수입니다.
        /// </summary>
        /// <returns>
        /// <see cref="Option{T}"/>이 값을 가지면 해당 값의 해시값,
        /// 아니면 <c>0</c>입니다.
        /// </returns>
        public override int GetHashCode() =>
            _hasValue ? EqualityComparer<T>.Default.GetHashCode(_value) : 0;

        /// <summary>
        /// 이 <see cref="Option{T}"/>과 다른 개체가 동일한지 여부를 반환합니다.
        /// </summary>
        /// <param name="obj">비교할 개체입니다.</param>
        /// <returns>
        /// 이 <see cref="Option{T}"/>과 <paramref name="obj"/>가 동일하면
        /// <c>true</c>, 아니면 <c>false</c>입니다.
        /// </returns>
        public override bool Equals(object obj) =>
            obj is Option<T> ? Equals((Option<T>)obj) : false;

        /// <summary>
        /// 두 <see cref="Option{T}"/>이 동일한지 비교합니다.
        /// </summary>
        /// <param name="x">
        /// 첫번째 비교 대상 <see cref="Option{T}"/>입니다.
        /// </param>
        /// <param name="y">
        /// 두번째 비교 대상 <see cref="Option{T}"/>입니다.
        /// </param>
        /// <returns>
        /// <paramref name="x"/>와 <paramref name="y"/>가 동일하면 <c>true</c>,
        /// 아니면 <c>false</c>입니다.
        /// </returns>
        public static bool operator ==(Option<T> x, Option<T> y) => x.Equals(y);

        /// <summary>
        /// 두 <see cref="Option{T}"/>이 동일하지 않은지 비교합니다.
        /// </summary>
        /// <param name="x">
        /// 첫번째 비교 대상 <see cref="Option{T}"/>입니다.
        /// </param>
        /// <param name="y">
        /// 두번째 비교 대상 <see cref="Option{T}"/>입니다.
        /// </param>
        /// <returns>
        /// <paramref name="x"/>와 <paramref name="y"/>가 동일하지 않으면
        /// <c>true</c>, 아니면 <c>false</c>입니다.
        /// </returns>
        public static bool operator !=(Option<T> x, Option<T> y) =>
            false == x.Equals(y);
    }
}
