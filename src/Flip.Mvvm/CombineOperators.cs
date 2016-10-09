using System;
using System.Diagnostics.CodeAnalysis;

namespace Flip
{
    public static class CombineOperators
    {
        public static bool Or(bool arg1, bool arg2) => arg1 || arg2;

        public static bool Or(bool arg1, bool arg2, bool arg3)
            => Or(arg1, arg2) || arg3;

        [SuppressMessage("Microsoft.Design", "CA1025:ReplaceRepetitiveArgumentsWithParamsArray", Justification = "The purpose of this method is to use to Observable.CombineLatest() method as 'resultSelector' parameter")]
        public static bool Or(bool arg1, bool arg2, bool arg3, bool arg4)
            => Or(arg1, arg2, arg3) || arg4;

        public static bool And(bool arg1, bool arg2) => arg1 && arg2;

        public static bool And(bool arg1, bool arg2, bool arg3)
            => And(arg1, arg2) && arg3;

        [SuppressMessage("Microsoft.Design", "CA1025:ReplaceRepetitiveArgumentsWithParamsArray", Justification = "The purpose of this method is to use to Observable.CombineLatest() method as 'resultSelector' parameter")]
        public static bool And(bool arg1, bool arg2, bool arg3, bool arg4)
            => And(arg1, arg2, arg3) && arg4;
    }
}
