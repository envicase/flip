using System;

namespace Flip.Tests
{
    public class UserCoalescable : User, ICoalescable<UserCoalescable>
    {
        public UserCoalescable(
            Guid id,
            string userName,
            Option<string>? bio,
            Option<string>? email)
            : base(id, userName, bio, email)
        {
        }

        public UserCoalescable Coalesce(UserCoalescable right)
        {
            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (right.Id != Id)
            {
                var message = $"{nameof(right)}.{nameof(right.Id)} is invalid.";
                throw new ArgumentException(message, nameof(right));
            }

            if (UserName != null && Bio != null)
                return this;

            return new UserCoalescable(
                Id,
                UserName ?? right.UserName,
                Bio ?? right.Bio,
                Email ?? right.Email);
        }
    }
}
