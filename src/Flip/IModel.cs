namespace Flip
{
    using System;

    public interface IModel<TId>
        where TId : IEquatable<TId>
    {
        TId Id { get; }
    }
}
