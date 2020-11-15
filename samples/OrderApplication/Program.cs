using System;
using System.Threading.Tasks;
using Mictor;

namespace OrderApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            var orderId = Guid.NewGuid().ToString();

            // simulate 2 requests

            FirstRequest(orderId);
            SecondRequest(orderId);

            Console.ReadLine();
        }

        private static void FirstRequest(string orderId)
        {
            using (IActor orderActor = ActorPool.Shared.GetOrCreate(orderId))
            {
                orderActor.Enqueue(async () =>
                {
                    Console.WriteLine("Getting order from db...");

                    await Task.Delay(5000);

                    Console.WriteLine("Successfully got order from db");
                });
            }
        }

        private static void SecondRequest(string orderId)
        {
            using (IActor orderActor = ActorPool.Shared.GetOrCreate(orderId))
            {
                orderActor.Enqueue(() =>
                {
                    Console.WriteLine("Updating order");

                    return Task.CompletedTask;
                });
            }
        }
    }
}
