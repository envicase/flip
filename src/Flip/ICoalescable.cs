namespace Flip
{
    public interface ICoalescable<T>
        where T : class
    {
        T Coalesce(T other);
    }
}
