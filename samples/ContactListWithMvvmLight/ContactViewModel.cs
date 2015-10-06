using System;
using System.Reactive.Linq;
using Flip;
using GalaSoft.MvvmLight;

namespace ContactListWithMvvmLight
{
    public class ContactViewModel : ViewModelBase
    {
        private readonly IConnection<Contact, int> _connection;
        private Contact _model;

        public ContactViewModel(Contact user)
        {
            _connection = Stream<Contact, int>.Connect(user.Id);
            _connection.Subscribe(m => Model = m);
            _model = user;
        }

        protected IConnection<Contact, int> Connection => _connection;

        public Contact Model
        {
            get { return _model; }
            private set { Set(ref _model, value); }
        }
    }
}
