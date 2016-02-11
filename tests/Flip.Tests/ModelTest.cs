using System;
using FluentAssertions;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace Flip.Tests
{
    public class ModelTest
    {
        [Theory, AutoData]
        public void SetsIdCorrectly(
            Guid id, string userName, string bio, string email) =>
            new User(id, userName, bio, email).Id.Should().Be(id);
    }
}
