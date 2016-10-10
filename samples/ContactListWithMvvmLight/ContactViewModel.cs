using System;
using System.Reactive.Linq;
using Flip;
using GalaSoft.MvvmLight;

namespace ContactListWithMvvmLight
{
    public class ContactViewModel : ViewModelBase
    {
        public static readonly StreamFactory<int, Contact> StreamFactory =
               new StreamFactory<int, Contact>();

        private readonly IConnection<int, Contact> _connection;
        private Contact _model;

        public ContactViewModel(Contact model)
        {
            _connection = StreamFactory.Connect(model.Id);
            _connection.Subscribe(m => Model = m);
            _connection.Emit(model);
        }

        protected IConnection<Contact> Connection => _connection;

        public Contact Model
        {
            get { return _model; }
            private set { Set(ref _model, value); }
        }
    }
}
