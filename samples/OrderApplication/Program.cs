using System;
using System.Threading.Tasks;
using Mictor;

var orderId = Guid.NewGuid().ToString();

// simulate 2 requests

FirstRequest(orderId);
SecondRequest(orderId);

PrintSnapshot();

Console.ReadLine();
PrintSnapshot();


void PrintSnapshot()
{
    ActorPoolSnapshot snapshot = ActorPool.Shared.TakeSnapshot();

    foreach ((string key, ActorSnapshot actorSnapshot) in snapshot)
    {
        Console.WriteLine($"{key} => {actorSnapshot}");
    }
}

void FirstRequest(string orderId)
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

void SecondRequest(string orderId)
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
