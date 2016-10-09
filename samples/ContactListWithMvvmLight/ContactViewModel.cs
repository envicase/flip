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

        private Contact _model;

        public ContactViewModel(Contact model)
        {
            Connection = StreamFactory.Connect(model.Id);
            Connection.Subscribe(m => Model = m);
            Connection.Emit(model);
        }

        protected IConnection<Contact> Connection { get; }

        public Contact Model
        {
            get { return _model; }
            private set { Set(ref _model, value); }
        }
    }
}
