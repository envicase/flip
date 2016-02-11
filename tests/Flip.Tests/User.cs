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

        public string UserName { get; set; }

        public Option<string>? Bio { get; set; }

        public Option<string>? Email { get; set; }
    }
}
