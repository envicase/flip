namespace Flip
{
    public interface IStreamFilter<TModel>
        where TModel : class
    {
        TModel Execute(TModel newValue, TModel lastValue);
    }
}
