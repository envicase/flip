using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Flip
{
    /// <summary>
    /// <see cref="ICoalescer{T}"/> 인터페이스 구현체의
    /// 추상 기반 클래스를 제공합니다.
    /// </summary>
    /// <typeparam name="T">상태를 유착할 개체의 형식입니다.</typeparam>
    public class Coalescer<T> : ICoalescer<T>
        where T : class
    {
        /// <summary>
        /// <see cref="ICoalescer{T}"/> 인터페이스의 기본 구현제를 가져옵니다.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Coalescer<T> class should provide default implementation of ICoalescer<T> without instantiation.")]
        public static Coalescer<T> Default { get; } = new Coalescer<T>();

        private static Func<T, T, T> _implementor;

        private static Func<T, T, T> Implementor =>
            _implementor == null
                ? (_implementor = GetImplementor())
                : _implementor;

        private static Func<T, T, T> GetImplementor()
        {
            Func<T, T, T> factory = typeof(T).GetTypeInfo()
                .DeclaredConstructors
                .Select(TryGetFactory)
                .FirstOrDefault(x => x != null);

            return (left, right) =>
            {
                var coalescable = left as ICoalescable<T>;
                if (coalescable != null)
                    return coalescable.Coalesce(right);

                if (factory == null)
                    throw new InvalidOperationException();

                return factory.Invoke(left, right);
            };
        }

        private static bool IsNullable(TypeInfo type) =>
            type.IsGenericType &&
            type.GetGenericTypeDefinition() == typeof(Nullable<>);

        private static Func<T, T, T> TryGetFactory(ConstructorInfo constructor)
        {
            var properties = new HashSet<PropertyInfo>(
                from property in typeof(T).GetRuntimeProperties()
                where property.CanRead
                select property);

            ParameterExpression leftParameterExpression =
                Expression.Parameter(typeof(T), "left");
            ParameterExpression rightParameterExpression =
                Expression.Parameter(typeof(T), "right");

            Expression[] arguments = constructor.GetParameters()
                .Select(parameter =>
                {
                    PropertyInfo property = FindProperty(parameter, properties);

                    if (property == null)
                        return null;

                    properties.Remove(property);

                    TypeInfo propertyType = property.PropertyType.GetTypeInfo();

                    return propertyType.IsClass || IsNullable(propertyType)
                        ? Expression.MakeBinary(
                            ExpressionType.Coalesce,
                            Expression.MakeMemberAccess(
                                leftParameterExpression,
                                property),
                            Expression.MakeMemberAccess(
                                rightParameterExpression,
                                property))
                        : Expression.MakeMemberAccess(
                            leftParameterExpression,
                            property) as Expression;
                })
                .ToArray();

            if (arguments.Any(a => a == null))
                return null;

            if (properties.Any(p => false == p.CanWrite))
                return null;

            NewExpression newExpression =
                Expression.New(constructor, arguments);

            MemberInitExpression memberInitExpression =
                Expression.MemberInit(
                    newExpression,
                    from property in properties
                    select Expression.Bind(
                        property,
                        Expression.MakeBinary(
                            ExpressionType.Coalesce,
                            Expression.MakeMemberAccess(
                                leftParameterExpression,
                                property),
                            Expression.MakeMemberAccess(
                                rightParameterExpression,
                                property))));

            Expression<Func<T, T, T>> lambdaExpression =
                Expression.Lambda<Func<T, T, T>>(
                    memberInitExpression,
                    leftParameterExpression,
                    rightParameterExpression);

            return lambdaExpression.Compile();
        }

        private static PropertyInfo FindProperty(
            ParameterInfo parameter, HashSet<PropertyInfo> properties) =>
            properties.FirstOrDefault(p => ParameterMatch(p, parameter));

        private static bool ParameterMatch(
            PropertyInfo property, ParameterInfo parameter) =>
            ParameterTypeMatch(property, parameter) &&
            ParameterNameMatch(property, parameter);

        private static bool ParameterTypeMatch(
            PropertyInfo property, ParameterInfo parameter) =>
            parameter.ParameterType == property.PropertyType;

        private static bool ParameterNameMatch(
            PropertyInfo property, ParameterInfo parameter) =>
            string.Equals(
                parameter.Name,
                property.Name,
                StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// 두 개체 상태의 유착 결과를 반환합니다.
        /// </summary>
        /// <param name="left">
        /// 첫번째 피연산자입니다. 구현체는 이 연산자가 가진 상태가
        /// 우선 순위를 갖도록 기능을 제공합니다.
        /// </param>
        /// <param name="right">
        /// 두번째 피연산자입니다. 구현체는 이 연산자가 <paramref name="left"/>가
        /// 가지지 않은 상태에 대해 대체값을 제공하도록 기능을 제공합니다.
        /// </param>
        /// <returns><paramref name="left"/>와 <paramref name="right"/>의
        /// 상태를 유착한 결과입니다.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>매개변수 <paramref name="left"/>가 <c>null</c>인 경우</para>
        /// <para>- 또는 -</para>
        /// <para>매개변수 <paramref name="right"/>가 <c>null</c>인 경우</para>
        /// </exception>
        public virtual T Coalesce(T left, T right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));
            if (right == null)
                throw new ArgumentNullException(nameof(right));

            return Implementor.Invoke(left, right);
        }
    }
}
