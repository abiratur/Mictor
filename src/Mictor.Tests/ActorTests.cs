using FluentAssertions;
using NSubstitute;
using System;
using Xunit;

namespace Mictor.Tests
{
    public class ActorTests
    {
        private readonly string _key = "1";
        private readonly Action<Actor> _onActorTerminated = Substitute.For<Action<Actor>>();

        [Fact]
        public void create_should_create_a_single_reference()
        {
            // act
            Actor actor = Actor.Create(_onActorTerminated, _key, out ActorReference reference);

            // assert
            actor.Should().NotBeNull();
            reference.Should().NotBeNull();

            actor.TakeSnapshot().References.Should().Be(1);

            _onActorTerminated.DidNotReceiveWithAnyArgs()(default);
        }
    }
}
