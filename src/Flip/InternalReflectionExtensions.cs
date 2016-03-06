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
            /*
             * TODO: #13
             * https://github.com/envicase/flip/issues/13
             * 이 메서드가 구현되면 제네릭 형식에 대해서 C# 문법을 따르는
             * 생성자 이름을 반환해야 합니다.
             */

            return type.Name;
        }

        public static string GetFriendlyName(this ConstructorInfo constructor)
        {
            /*
             * TODO: #14
             * https://github.com/envicase/flip/issues/14
             * 이 메서드가 구현되면 제네릭 형식에 대해서 C# 문법을 따르는
             * 읽기 용이한 생성자 오버로드 이름을 반환해야 합니다.
             */

            return constructor.ToString();
        }

        private static string GetNameWithoutGenericDefinition(Type type)
            => type.Name.Substring(0, type.Name.IndexOf('`'));
    }
}
