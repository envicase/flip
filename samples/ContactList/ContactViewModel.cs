using System;
using Flip.ViewModels;

namespace ContactList
{
    public class ContactViewModel : ReactiveViewModel<Contact, int>
    {
        public ContactViewModel(Contact model)
            : base(model)
        {
        }
    }
}
