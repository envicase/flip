using System;
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
            /*
             * TODO: #12
             * https://github.com/envicase/flip/issues/12
             * 이 메서드가 구현되면 제네릭 형식에 대해서 C# 문법을 따르는
             * 읽기 용이한 형 이름을 반환해야 합니다.
             */

            return type.Name;
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
    }
}
