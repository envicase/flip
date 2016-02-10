using System;
using System.Collections.Generic;

namespace Flip
{
    public static class Option
    {
        public static Option<T> Create<T>(T value) =>
            value == null ? default(Option<T>) : new Option<T>(value);
    }

    public struct Option<T> : IEquatable<Option<T>>
    {
        private readonly bool _hasValue;
        private readonly T _value;

        public Option(T value)
        {
            _hasValue = value != null;
            _value = value;
        }

        public bool HasValue => _hasValue;

        public T ValueOrDefault => _hasValue ? _value : default(T);

        public static implicit operator Option<T>(T value)
        {
            return Option.Create(value);
        }

        public bool Equals(Option<T> other)
        {
            return _hasValue == other._hasValue
                ? _hasValue
                    ? EqualityComparer<T>.Default.Equals(_value, other._value)
                    : true
                : false;
        }
    }
}
