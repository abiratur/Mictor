using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Mictor.Tests
{
    public class ActorPoolTests
    {
        [Fact]
        public void active_actor_should_have_reference()
        {
            // arrange
            var target = new ActorPool();

            // act
            using var actor = target.GetOrCreate(Guid.NewGuid().ToString());

            // assert
            target.TakeSnapshot().Count.Should().Be(1);
        }

        [Fact]
        public void active_actor_should_have_multiple_reference()
        {
            // arrange
            var target = new ActorPool();

            var id = Guid.NewGuid().ToString();

            // act
            using var actor = target.GetOrCreate(id);
            using var actor2 = target.GetOrCreate(id);

            // assert
            ActorPoolSnapshot snapshot = target.TakeSnapshot();
            snapshot.Count.Should().Be(1);
            snapshot[id].QueuedWork.Should().Be(0);
            snapshot[id].References.Should().Be(2);
        }

        [Fact]
        public void inactive_actor_should_not_have_reference()
        {
            // arrange
            var target = new ActorPool();

            // act
            using (var actor = target.GetOrCreate(Guid.NewGuid().ToString()))
            {
            }

            // assert
            target.TakeSnapshot().Count.Should().Be(1);
        }

        [Fact]
        public void multiple_actor_references_should_run_sequentially()
        {
            // arrange
            var random = new Random();

            var target = new ActorPool();

            int count = 0;

            async Task Work()
            {
                int temp = count;

                temp++;
                await Task.Yield();

                count = temp;
            }

            var id = Guid.NewGuid().ToString();

            // act
            Parallel.For(0, 100, _ =>
            {
                using (IActor actor = target.GetOrCreate(id))
                {
                    Thread.Sleep(random.Next(10));
                    actor.Enqueue(Work);
                }
            });

            // assert
            Thread.Sleep(5);
            count.Should().Be(100);
        }
    }
}
