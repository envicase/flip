using System;
using System.Reactive.Linq;
using Flip;
using GalaSoft.MvvmLight;

namespace ContactListWithMvvmLight
{
    public class ContactViewModel : ViewModelBase
    {
        private Contact _model;

        public ContactViewModel(int userId)
        {
            Connection = Stream<Contact, int>.Connect(userId);
            Connection.Subscribe(m => Model = m);
        }

        protected IConnection<Contact, int> Connection { get; }

        public Contact Model
        {
            get { return _model; }
            private set { Set(ref _model, value); }
        }
    }
}
