using System;

namespace Flip.Tests
{
    public interface IFunctor
    {
        void Action<T>(T arg);

        TResult Func<T, TResult>(T arg);
    }
}
