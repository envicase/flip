namespace Flip
{
    using System;
    using FluentAssertions;
    using Ploeh.AutoFixture;
    using Xunit;

    public class CoalescingFilter_features
    {
        public class UserModel : Model<Guid>
        {
            public UserModel(Guid id, string userName, string bio)
                : base(id)
            {
                UserName = userName;
                Bio = bio;
            }

            public string UserName { get; }

            public string Bio { get; }

            public string Website { get; set; }
        }

        public class CoalescableUserModel :
            UserModel,
            ICoalescable<CoalescableUserModel>
        {
            public CoalescableUserModel(Guid id, string userName, string bio)
                : base(id, userName, bio)
            {
            }

            public CoalescableUserModel Coalesce(CoalescableUserModel other)
            {
                return new CoalescableUserModel(
                    Id,
                    UserName ?? other.UserName,
                    Bio ?? other.Bio)
                {
                    Website = Website ?? other.Website
                };
            }
        }

        private readonly IFixture _fixture = new Fixture();

        [Fact]
        public void Execute_uses_property_of_new_value_if_not_null()
        {
            // Arrange
            var last = _fixture.Create<UserModel>();
            var @new = _fixture.Create<UserModel>();

            var sut = new CoalescingFilter<UserModel>();

            // Act
            UserModel actual = sut.Execute(@new, last);

            // Assert
            actual.Should().NotBeSameAs(@new);
            actual.ShouldBeEquivalentTo(@new);
        }

        [Fact]
        public void Execute_uses_property_of_last_value_if_of_new_value_is_null()
        {
            // Arrange
            var last = _fixture.Create<UserModel>();
            var @new = new UserModel(
                last.Id, _fixture.Create<string>(), bio: null);

            var sut = new CoalescingFilter<UserModel>();

            // Act
            UserModel actual = sut.Execute(@new, last);

            // Assert
            actual.ShouldBeEquivalentTo(new
            {
                @new.Id,
                @new.UserName,
                last.Bio,
                last.Website
            });
        }

        [Fact]
        public void Execute_relays_to_ICoalescable()
        {
            // Arrange
            var last = _fixture.Create<CoalescableUserModel>();
            var @new = new CoalescableUserModel(
                last.Id, _fixture.Create<string>(), bio: null);

            var sut = new CoalescingFilter<CoalescableUserModel>();

            // Act
            CoalescableUserModel actual = sut.Execute(@new, last);

            // Assert
            actual.ShouldBeEquivalentTo(new
            {
                @new.Id,
                @new.UserName,
                last.Bio,
                last.Website
            });
        }
    }
}
