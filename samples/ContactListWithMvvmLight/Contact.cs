using System;
using Flip;

namespace ContactListWithMvvmLight
{
    public class Contact : Model<int>
    {
        public Contact(int id, string name, string email)
            : base(id)
        {
            Name = name;
            Email = email;
        }

        public string Name { get; }

        public string Email { get; }
    }
}
