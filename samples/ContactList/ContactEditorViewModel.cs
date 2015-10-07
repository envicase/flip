using System;
using System.Reactive.Linq;
using Flip;
using Flip.ViewModels;

namespace ContactList
{
    using static CombineOperators;

    public class ContactEditorViewModel : ReactiveViewModel<Contact, int>
    {
        private string _editName;
        private string _editEmail;

        public ContactEditorViewModel(Contact user)
            : base(user)
        {
            this.Observe(x => x.Model).Subscribe(_ => ProjectModel());
        }

        private void ProjectModel()
        {
            EditName = Model.Name;
            EditEmail = Model.Email;
        }

        public string EditName
        {
            get { return _editName; }
            set { SetValue(ref _editName, value); }
        }

        public string EditEmail
        {
            get { return _editEmail; }
            set { SetValue(ref _editEmail, value); }
        }

        private IObservable<bool> HasChanges =>
            this.Observe(c => c.Model, _ => false).Merge(
                Observable.CombineLatest(
                    this.Observe(c => c.EditName, p => p != Model.Name),
                    this.Observe(c => c.EditEmail, p => p != Model.Email), Or));

        private bool HasValue(string s) => !string.IsNullOrWhiteSpace(s);

        public IReactiveCommand RestoreCommand =>
            ReactiveCommand.Create(HasChanges, _ => ProjectModel());

        public IReactiveCommand SaveCommand => ReactiveCommand.Create(
            HasChanges.CombineLatest(
                this.Observe(c => c.EditName, HasValue),
                this.Observe(c => c.EditEmail, HasValue), And),
            _ => Connection.Emit(new Contact(ModelId, EditName, EditEmail)));
    }
}
