using Flip;
using Flip.ViewModels;

namespace ContactList
{
    public class ContactViewModel : ReactiveViewModel<int, Contact>
    {
        public static readonly StreamFactory<int, Contact> StreamFactory =
               new StreamFactory<int, Contact>();

        public ContactViewModel(Contact model)
            : base(() => StreamFactory.Connect(model.Id))
        {
            Connection.Emit(model);
        }
    }
}
