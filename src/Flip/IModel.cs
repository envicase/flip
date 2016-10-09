using System;

namespace Flip
{
    public interface IModel<TId>
        where TId : IEquatable<TId>
    {
        TId Id { get; }
    }
}
