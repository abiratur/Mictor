using System;
using System.Collections.Generic;
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

            PrintSnapshot();

            Console.ReadLine();

            PrintSnapshot();
        }

        private static void PrintSnapshot()
        {
            ActorPoolSnapshot snapshot = ActorPool.Shared.TakeSnapshot();

            foreach (KeyValuePair<string, ActorSnapshot> kvp in snapshot)
            {
                Console.WriteLine($"{kvp.Key} => {kvp.Value}");
            }
        }

        private static void FirstRequest(string orderId)
        {
            Console.WriteLine("Getting actor first request...");
            using (IActor orderActor = ActorPool.Shared.GetOrCreate(orderId))
            {
                Console.WriteLine("Adding first request...");
                orderActor.Enqueue(async () =>
                {
                    Console.WriteLine("Getting order from db...");

                    await Task.Delay(5000);

                    Console.WriteLine("Successfully got order from db");
                });
            }

            Console.WriteLine("Disposed actor first request");
        }

        private static void SecondRequest(string orderId)
        {
            Console.WriteLine("Getting actor second request...");
            using (IActor orderActor = ActorPool.Shared.GetOrCreate(orderId))
            {
                Console.WriteLine("Adding second request...");
                orderActor.Enqueue(() =>
                {
                    Console.WriteLine("Updating order");

                    return Task.CompletedTask;
                });
            }
        }
    }
}
