using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Windows;
using Flip;

namespace ContactList
{
    using static Visibility;

    public class MainViewModel : ObservableObject
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
            var scheduler = DispatcherScheduler.Current;
            ReactiveCommand.SchedulerFactory = () => scheduler;

            Contacts = (from c in GetSampleData()
                        select new ContactViewModel(c)).ToList();

            this.Observe(c => c.SelectedItem)
                .Subscribe(i => Editor = i == null ? null :
                                new ContactEditorViewModel(i.Model));

            this.Observe(c => c.Editor)
                .Subscribe(_ =>
                {
                    OnPropertyChanged(nameof(EditorVisibility));
                    OnPropertyChanged(nameof(GuideMessageVisibility));
                });
        }

        public IEnumerable<ContactViewModel> Contacts { get; }

        public ContactViewModel SelectedItem
        {
            get { return _selectedItem; }
            set { SetValue(ref _selectedItem, value); }
        }

        public ContactEditorViewModel Editor
        {
            get { return _editor; }
            private set { SetValue(ref _editor, value); }
        }

        public Visibility EditorVisibility =>
            _editor == null ? Hidden : Visible;

        public Visibility GuideMessageVisibility =>
            _editor == null ? Visible : Hidden;
    }
}
