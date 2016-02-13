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
             * TODO: 아직 구현되지 않았습니다.
             * 이 메서드가 구현되면 제네릭 형식에 대해서 C# 문법을 따르는
             * 읽기 용이한 형 이름을 반환해야 합니다. 예를 들어
             * Dictionary<int, string> 형식에 대해서 'Dictionary`2'가 아닌
             * 'Dictionary<int, string>'가 반환되어야 합니다.
             */

            return type.Name;
        }

        public static string GetConstructorName(this Type type)
        {
            /*
             * TODO: 아직 구현되지 않았습니다.
             * 이 메서드가 구현되면 제네릭 형식에 대해서 C# 문법을 따르는
             * 생성자 이름을 반환해야 합니다. 예를 들어
             * Dictionary<int, string> 형식에 대해서 '.ctor'나
             * 'Dictionary`2'가 아닌 'Dictionary'가 반환되어야 합니다.
             */

            return type.Name;
        }

        public static string GetFriendlyName(this ConstructorInfo constructor)
        {
            /*
             * TODO: 아직 구현되지 않았습니다.
             * 이 메서드가 구현되면 제네릭 형식에 대해서 C# 문법을 따르는
             * 읽기 용이한 형 이름을 반환해야 합니다. 예를 들어
             * public List(IEnumerable<int> collection) 생성자에 대해서
             * 'Void .ctor(System.Collections.Generic.IEnumerable`1[System.Int32])'가
             * 아닌 'public List(IEnumerable<int> collection)'가
             * 반환되어야 합니다.
             */

            return constructor.ToString();
        }
    }
}
