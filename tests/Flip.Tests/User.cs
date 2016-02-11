using System;

namespace Flip.Tests
{
    public class User : Model<Guid>
    {
        public User(
            Guid id,
            string userName,
            Option<string>? bio,
            Option<string>? email)
            : base(id)
        {
            if (userName == null)
                throw new ArgumentNullException(nameof(userName));

            UserName = userName;
            Bio = bio;
            Email = email;
        }

        public string UserName { get; }

        public Option<string>? Bio { get; }

        public Option<string>? Email { get; }

        public Option<string>? Website { get; set; }
    }
}
