using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using static System.Linq.Expressions.Expression;

namespace Flip
{
    internal class InitExpressionFactory<T>
    {
        private static PropertyInfo Pick(
            HashSet<PropertyInfo> properties,
            Func<PropertyInfo, bool> predicate)
        {
            PropertyInfo property = properties.FirstOrDefault(predicate);

            if (property != null)
                properties.Remove(property);

            return property;
        }

        private PropertyInfo PickParameterMatch(
            HashSet<PropertyInfo> properties, ParameterInfo parameter) =>
            Pick(properties, property => ParameterMatch(property, parameter));

        private bool ParameterMatch(
            PropertyInfo property, ParameterInfo parameter) =>
            ParameterTypeMatch(property, parameter) &&
            ParameterNameMatch(property, parameter);

        private bool ParameterTypeMatch(
            PropertyInfo property, ParameterInfo parameter) =>
            parameter.ParameterType == property.PropertyType;

        private bool ParameterNameMatch(
            PropertyInfo property, ParameterInfo parameter) =>
            string.Equals(
                parameter.Name,
                property.Name,
                StringComparison.OrdinalIgnoreCase);

        private static Expression CoalesceOrMemberAccess(
            PropertyInfo property,
            ParameterExpression left,
            ParameterExpression right)
        {
            return property.PropertyType.IsClass()
                || property.PropertyType.IsNullable()
                ? MakeBinary(
                    ExpressionType.Coalesce,
                    MakeMemberAccess(left, property),
                    MakeMemberAccess(right, property))
                : MakeMemberAccess(left, property) as Expression;
        }

        private InitExpressionResult<Func<T, T, T>> CreateCoalesceWith(
            ConstructorInfo constructor)
        {
            var readableProperties = new List<PropertyInfo>(
                from p in typeof(T).GetRuntimeProperties()
                where p.CanRead
                select p);

            var readonlyProperties = new HashSet<PropertyInfo>(
                from p in readableProperties
                where false == p.CanWrite
                select p);

            var settableProperties = new HashSet<PropertyInfo>(
                from p in readableProperties
                where p.CanWrite
                select p);

            ParameterExpression left = Parameter(typeof(T), nameof(left));
            ParameterExpression right = Parameter(typeof(T), nameof(right));

            var arguments = new List<Expression>();

            foreach (ParameterInfo parameter in constructor.GetParameters())
            {
                PropertyInfo property =
                    PickParameterMatch(readonlyProperties, parameter) ??
                    PickParameterMatch(settableProperties, parameter);

                if (property == null)
                {
                    InitExpressionError error =
                        GetNoPropertyMatchesParameterError(
                            constructor, parameter);
                    return InitExpressionResult<Func<T, T, T>>.WithError(error);
                }

                arguments.Add(CoalesceOrMemberAccess(property, left, right));
            }

            if (readonlyProperties.Any())
            {
                InitExpressionError error =
                    GetPropertyCannotBeSetError(
                        constructor, readonlyProperties);
                return InitExpressionResult<Func<T, T, T>>.WithError(error);
            }

            MemberInitExpression memberInitExpression = MemberInit(
                New(constructor, arguments),
                from property in settableProperties
                select Bind(
                    property,
                    CoalesceOrMemberAccess(property, left, right)));

            Expression<Func<T, T, T>> lambdaExpression =
                Lambda<Func<T, T, T>>(memberInitExpression, left, right);

            return new InitExpressionResult<Func<T, T, T>>(lambdaExpression);
        }

        private static InitExpressionError GetNoPropertyMatchesParameterError(
            ConstructorInfo constructor, ParameterInfo parameter)
        {
            var message = new StringBuilder();

            message
                .Append("No property found that matches ")
                .Append($"the parameter '{parameter.Name}'.")
                .AppendLine();

            return new InitExpressionError(constructor, message.ToString());
        }

        private static InitExpressionError GetPropertyCannotBeSetError(
            ConstructorInfo constructor,
            HashSet<PropertyInfo> readonlyProperties)
        {
            var message = new StringBuilder();

            foreach (PropertyInfo property in readonlyProperties)
            {
                message
                    .Append($"The property '{property.Name}' ")
                    .Append("cannot be set.")
                    .AppendLine();

                message
                    .Append("Consider make it settable or ")
                    .Append("able to be set through a constructor like ")
                    .Append($"'public {typeof(T).GetConstructorName()}(");
                foreach (ParameterInfo parameter in constructor.GetParameters())
                {
                    string parameterTypeName =
                        parameter.ParameterType.GetFriendlyName();
                    message
                        .Append($"{parameterTypeName} ")
                        .Append($"{parameter.Name}, ");
                }
                string propertyTypeName =
                    property.PropertyType.GetFriendlyName();
                message
                    .Append($"{propertyTypeName} ")
                    .Append(property.Name.Substring(0, 1).ToLower())
                    .Append(property.Name.Substring(1))
                    .Append(")'.")
                    .AppendLine();
            }

            return new InitExpressionError(constructor, message.ToString());
        }

        public InitExpressionResult<Func<T, T, T>> CreateCoalesce()
        {
            IEnumerable<ConstructorInfo> constructors =
                typeof(T).GetTypeInfo().DeclaredConstructors;

            var results = new List<InitExpressionResult<Func<T, T, T>>>(
                from constructor in constructors
                orderby constructor.GetParameters().Length descending
                select CreateCoalesceWith(constructor));

            return results.FirstOrDefault(r => r.IsSuccess)
                ?? InitExpressionResult<Func<T, T, T>>.WithErrors(
                    from r in results
                    from e in r.Errors
                    select e);
        }
    }
}
