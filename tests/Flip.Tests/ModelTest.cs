using System;
using FluentAssertions;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace Flip.Tests
{
    public class ModelTest
    {
        [Theory, AutoData]
        public void SetsIdCorrectly(string id, string userName, string bio) =>
            new User(id, userName, bio).Id.Should().BeSameAs(id);
    }
}
