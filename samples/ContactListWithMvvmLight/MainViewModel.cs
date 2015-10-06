using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using GalaSoft.MvvmLight;

namespace ContactListWithMvvmLight
{
    public class MainViewModel : ViewModelBase
    {
        private static IEnumerable<Contact> GetSampleData() => new[]
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
            Users = (from u in GetSampleData()
                     select new ContactViewModel(u)).ToList();
        }

        public IEnumerable<ContactViewModel> Users { get; }

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
            _editor == null ? Visibility.Hidden : Visibility.Visible;

        public Visibility GuideMessageVisibility =>
            _editor == null ? Visibility.Visible : Visibility.Hidden;
    }
}
