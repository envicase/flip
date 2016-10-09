namespace Flip
{
    using System;
    using System.Linq;

    public class CoalescingFilter<TModel> : IStreamFilter<TModel>
        where TModel : class
    {
        private static Func<TModel, TModel, TModel> _implementor;

        private static Func<TModel, TModel, TModel> Implementor =>
            _implementor == null
            ? (_implementor = BuildImplementor())
            : _implementor;

        public TModel Execute(TModel newValue, TModel lastValue)
        {
            if (newValue == null)
                throw new ArgumentNullException(nameof(newValue));

            return Implementor.Invoke(newValue, lastValue);
        }

        private static Func<TModel, TModel, TModel> BuildImplementor()
        {
            var factory = new InitExpressionFactory<TModel>();
            InitExpressionResult<Func<TModel, TModel, TModel>> result =
                factory.CreateCoalesce();

            return (newValue, lastValue) =>
            {
                var coalescable = newValue as ICoalescable<TModel>;
                if (coalescable != null)
                    return coalescable.Coalesce(lastValue);

                if (false == result.IsSuccess)
                {
                    var message = string.Join(
                        Environment.NewLine,
                        from error in result.Errors
                        select error.GetExceptionMessage());
                    throw new InvalidOperationException(message);
                }

                return result.Function.Invoke(newValue, lastValue);
            };
        }
    }
}
