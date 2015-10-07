using System;
using System.Diagnostics.CodeAnalysis;

namespace Flip
{
    /// <summary>
    /// 둘 이상의 값을 병합하는 연산을 제공합니다.
    /// </summary>
    public static class CombineOperators
    {
        /// <summary>
        /// <see cref="bool"/> 형식의 두 값에 대한
        /// 논리 OR 연산 결과를 반환합니다.
        /// </summary>
        /// <param name="arg1">첫번째 피연산자입니다.</param>
        /// <param name="arg2">두번째 피연산자입니다.</param>
        /// <returns>
        /// <paramref name="arg1"/>과 <paramref name="arg2"/>에 대한
        /// 논리 OR 연산 결과입니다.
        /// </returns>
        public static bool Or(bool arg1, bool arg2) => arg1 || arg2;

        /// <summary>
        /// <see cref="bool"/> 형식의 세 값에 대한
        /// 누적 논리 OR 연산 결과를 반환합니다.
        /// </summary>
        /// <param name="arg1">첫번째 피연산자입니다.</param>
        /// <param name="arg2">두번째 피연산자입니다.</param>
        /// <param name="arg3">세번째 피연산자입니다.</param>
        /// <returns>
        /// <paramref name="arg1"/>, <paramref name="arg2"/>,
        /// <paramref name="arg3"/>에 대한 누적 논리 OR 연산 결과입니다.
        /// </returns>
        public static bool Or(bool arg1, bool arg2, bool arg3)
            => Or(arg1, arg2) || arg3;

        /// <summary>
        /// <see cref="bool"/> 형식의 네 값에 대한
        /// 누적 논리 OR 연산 결과를 반환합니다.
        /// </summary>
        /// <param name="arg1">첫번째 피연산자입니다.</param>
        /// <param name="arg2">두번째 피연산자입니다.</param>
        /// <param name="arg3">세번째 피연산자입니다.</param>
        /// <param name="arg4">네번째 피연산자입니다.</param>
        /// <returns>
        /// <paramref name="arg1"/>, <paramref name="arg2"/>,
        /// <paramref name="arg3"/>, <paramref name="arg3"/>에 대한
        /// 누적 논리 OR 연산 결과입니다.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1025:ReplaceRepetitiveArgumentsWithParamsArray", Justification = "The purpose of this method is to use to Observable.CombineLatest() method as 'resultSelector' parameter")]
        public static bool Or(bool arg1, bool arg2, bool arg3, bool arg4)
            => Or(arg1, arg2, arg3) || arg4;

        /// <summary>
        /// <see cref="bool"/> 형식의 두 값에 대한
        /// 논리 AND 연산 결과를 반환합니다.
        /// </summary>
        /// <param name="arg1">첫번째 피연산자입니다.</param>
        /// <param name="arg2">두번째 피연산자입니다.</param>
        /// <returns>
        /// <paramref name="arg1"/>과 <paramref name="arg2"/>에 대한
        /// 논리 AND 연산 결과입니다.
        /// </returns>
        public static bool And(bool arg1, bool arg2) => arg1 && arg2;

        /// <summary>
        /// <see cref="bool"/> 형식의 세 값에 대한
        /// 누적 논리 AND 연산 결과를 반환합니다.
        /// </summary>
        /// <param name="arg1">첫번째 피연산자입니다.</param>
        /// <param name="arg2">두번째 피연산자입니다.</param>
        /// <param name="arg3">세번째 피연산자입니다.</param>
        /// <returns>
        /// <paramref name="arg1"/>, <paramref name="arg2"/>,
        /// <paramref name="arg3"/>에 대한 누적 논리 AND 연산 결과입니다.
        /// </returns>
        public static bool And(bool arg1, bool arg2, bool arg3)
            => And(arg1, arg2) && arg3;

        /// <summary>
        /// <see cref="bool"/> 형식의 네 값에 대한
        /// 누적 논리 AND 연산 결과를 반환합니다.
        /// </summary>
        /// <param name="arg1">첫번째 피연산자입니다.</param>
        /// <param name="arg2">두번째 피연산자입니다.</param>
        /// <param name="arg3">세번째 피연산자입니다.</param>
        /// <param name="arg4">네번째 피연산자입니다.</param>
        /// <returns>
        /// <paramref name="arg1"/>, <paramref name="arg2"/>,
        /// <paramref name="arg3"/>, <paramref name="arg4"/>에 대한
        /// 누적 논리 AND 연산 결과입니다.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1025:ReplaceRepetitiveArgumentsWithParamsArray", Justification = "The purpose of this method is to use to Observable.CombineLatest() method as 'resultSelector' parameter")]
        public static bool And(bool arg1, bool arg2, bool arg3, bool arg4)
            => And(arg1, arg2, arg3) && arg4;
    }
}
