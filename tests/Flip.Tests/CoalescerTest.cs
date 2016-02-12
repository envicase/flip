using System;
using FluentAssertions;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace Flip.Tests
{
    public class CoalescerTest
    {
        [Theory, AutoData]
        public void CoalesceDelegatesToCoalescable(UserCoalescable right)
        {
            var left = new UserCoalescable(
                right.Id, right.UserName, null, null);
            var sut = Coalescer<UserCoalescable>.Default;

            UserCoalescable actual = sut.Coalesce(left, right);

            actual.ShouldBeEquivalentTo(new
            {
                left.Id,
                left.UserName,
                right.Bio,
                right.Email,
                right.Website
            });
        }

        [Theory, AutoData]
        public void CoalesceUsesLeftPropertyValueIfLeftPropertyValueIsNotNull(
            User left, User right)
        {
            var sut = Coalescer<User>.Default;

            User actual = sut.Coalesce(left, right);

            actual.Should().NotBeSameAs(left);
            actual.ShouldBeEquivalentTo(left);
        }

        [Theory, AutoData]
        public void CoalesceUsesRightPropertyValueIfLeftPropertyValueIsNull(
            Guid id, string userName, string bio, string email, string website)
        {
            var left = new User(id, userName, null, null);
            var right = new User(id, userName, bio, email);
            right.Website = website;
            var sut = Coalescer<User>.Default;

            User actual = sut.Coalesce(left, right);

            actual.ShouldBeEquivalentTo(
                new User(id, userName, bio, email) { Website = website });
        }
    }
}
