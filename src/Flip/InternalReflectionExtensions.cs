using System;
using System.Linq;
using System.Reflection;

namespace Flip
{
    internal static class InternalReflectionExtensions
    {
        public static bool IsClass(this Type type) =>
            type.GetTypeInfo().IsClass;

        public static bool IsNullable(this Type type)
        {
            TypeInfo typeInfo = type.GetTypeInfo();
            return typeInfo.IsGenericType
                && typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static string GetFriendlyName(this Type type)
        {
            var argumentNames = string.Join(
                ", ", 
                from t in type.GenericTypeArguments
                select t.GetFriendlyName());
            return string.IsNullOrEmpty(argumentNames)
                ? type.Name
                : $"{GetNameWithoutGenericDefinition(type)}<{argumentNames}>";
        }

        public static string GetConstructorName(this Type type)
        {
            return type.GenericTypeArguments.Any()
                ? GetNameWithoutGenericDefinition(type)
                : type.Name;
        }

        public static string GetFriendlyName(this ConstructorInfo constructor)
        {
            var parameterDefinitions = string.Join(
                ", ",
                from p in constructor.GetParameters()
                select $"{p.ParameterType.GetFriendlyName()} {p.Name}");
            var typeName = constructor.DeclaringType.GetConstructorName();
            return $"{typeName}({parameterDefinitions})";
        }

        private static string GetNameWithoutGenericDefinition(Type type)
            => type.Name.Substring(0, type.Name.IndexOf('`'));
    }
}
