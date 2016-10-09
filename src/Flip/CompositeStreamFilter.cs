namespace Flip
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class CompositeStreamFilter<TModel> : IStreamFilter<TModel>
        where TModel : class
    {
        public CompositeStreamFilter(params IStreamFilter<TModel>[] filters)
            : this(filters?.AsEnumerable())
        {
        }

        public CompositeStreamFilter(IEnumerable<IStreamFilter<TModel>> filters)
        {
            if (null == filters)
                throw new ArgumentNullException(nameof(filters));

            List<IStreamFilter<TModel>> filterList = filters.ToList();
            for (int i = 0; i < filterList.Count; i++)
            {
                if (null == filterList[i])
                {
                    throw new ArgumentException(
                        $"{nameof(filters)} cannot contain null.",
                        nameof(filters));
                }
            }

            Filters = new ReadOnlyCollection<IStreamFilter<TModel>>(filterList);
        }

        public IEnumerable<IStreamFilter<TModel>> Filters { get; }

        public TModel Execute(TModel newValue, TModel lastValue)
        {
            if (null == newValue)
                throw new ArgumentNullException(nameof(newValue));
            if (null == lastValue)
                throw new ArgumentNullException(nameof(lastValue));

            TModel value = newValue;
            var filters = (IList<IStreamFilter<TModel>>)Filters;
            for (int i = 0; i < filters.Count; i++)
            {
                IStreamFilter<TModel> filter = filters[i];
                value = filter.Execute(value, lastValue);
                if (value == null)
                    break;
            }

            return value;
        }
    }
}
