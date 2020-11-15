using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace Mictor.Tests
{
    public class ActorPoolTests
    {
        [Test]
        public void GetOrCreateTest()
        {
            var target = new ActorPool();

            var countdownEvent = new CountdownEvent(3);

            using (IActor actor = target.GetOrCreate("a"))
            {
                Task Func()
                {
                    countdownEvent.Signal();
                    return Task.CompletedTask;
                }

                actor.Enqueue(Func);
                actor.Enqueue(Func);
                actor.Enqueue(Func);
            }

            countdownEvent.Wait(TimeSpan.FromSeconds(1)).Should().Be(true);

            Thread.Sleep(100);

            target.EstimatedCount.Should().Be(0);
        }

        [Test]
        public void GetOrCreateShouldReturnWorkingActorWithNoHandles()
        {
            var target = new ActorPool();

            var e = new ManualResetEventSlim(false);

            Task Work()
            {
                e.Wait();
                return Task.CompletedTask;
            }

            using (var actor = target.GetOrCreate("a"))
            {
                actor.Enqueue(Work);
            }

            // this should not block
            using var temp = target.GetOrCreate("a");
        }

        [Test]
        public void ParallelTest()
        {
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

            Parallel.For(0, 100, _ =>
            {
                using (var worker = target.GetOrCreate("a"))
                {
                    Thread.Sleep(random.Next(50));
                    worker.Enqueue(Work);
                }
            });

            Thread.Sleep(5);
            count.Should().Be(100);
        }
    }
}