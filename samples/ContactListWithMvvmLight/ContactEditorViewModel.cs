using System;
using System.Windows.Input;
using Flip;
using GalaSoft.MvvmLight.Command;

namespace ContactListWithMvvmLight
{
    public class ContactEditorViewModel : ContactViewModel
    {
        private string _editName;
        private string _editEmail;

        public ContactEditorViewModel(Contact user)
            : base(user)
        {
            _editName = user.Name;
            _editEmail = user.Email;
        }

        public string EditName
        {
            get { return _editName; }
            set { Set(ref _editName, value); }
        }

        public string EditEmail
        {
            get { return _editEmail; }
            set { Set(ref _editEmail, value); }
        }

        public ICommand SaveCommand => new RelayCommand(() =>
            Connection.Emit(new Contact(Model.Id, _editName, _editEmail)));
    }
}
