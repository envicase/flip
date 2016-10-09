using System.Collections.Generic;
using System.Linq;
using System.Windows;
using GalaSoft.MvvmLight;
using static System.Windows.Visibility;

namespace ContactListWithMvvmLight
{
    public class MainViewModel : ViewModelBase
    {
        private static Contact[] GetSampleData() => new[]
        {
            new Contact(1, "Tony Stark", "ironman@avengers.com"),
            new Contact(2, "Bruce Banner", "hulk@avengers.com"),
            new Contact(3, "Thor Odinson", "thor@avengers.com"),
            new Contact(4, "Steve Rogers", "captain@avengers.com"),
        };

        private ContactViewModel _selectedItem;
        private ContactEditorViewModel _editor;

        public MainViewModel()
        {
            Contacts = new List<ContactViewModel>(
                from contact in GetSampleData()
                select new ContactViewModel(contact));
        }

        public IEnumerable<ContactViewModel> Contacts { get; }

        public ContactEditorViewModel Editor
        {
            get { return _editor; }

            private set
            {
                if (Set(ref _editor, value))
                {
                    RaisePropertyChanged(nameof(EditorVisibility));
                    RaisePropertyChanged(nameof(GuideMessageVisibility));
                }
            }
        }

        public ContactViewModel SelectedItem
        {
            get { return _selectedItem; }

            set
            {
                if (Set(ref _selectedItem, value))
                {
                    Editor = _selectedItem == null ?
                        null : new ContactEditorViewModel(_selectedItem.Model);
                }
            }
        }

        public Visibility EditorVisibility =>
            _editor == null ? Hidden : Visible;

        public Visibility GuideMessageVisibility =>
            _editor == null ? Visible : Hidden;
    }
}
