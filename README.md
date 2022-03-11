
# Mictor

[![Build](https://github.com/abiratur/Mictor/workflows/dotnet/badge.svg?branch=main)](https://github.com/abiratur/Mictor/actions)
[![Coverage](https://codecov.io/gh/abiratur/Mictor/branch/main/graph/badge.svg)](https://codecov.io/gh/abiratur/Mictor)

Mictor is a micro library that implements the [Actor Model](https://en.wikipedia.org/wiki/Actor_model) in memory.

## Why?
Mictor tries to simplify working on concurrent applications, by using a simple actor model. 

## Download

- [NuGet](https://nuget.org/packages/Mictor): `dotnet add package Mictor`

## Usage
```csharp
using Mictor;

// obtain a reference to an actor based on a string key
using (var actorRef = ActorPool.Shared.GetOrCreate("id1"))
{
	// enqueue some work for the actor
	actor.Enqueue(async () => 
	{
		var someObj = await GetFromDbAsync();
		// update some obj ...
		await UpdateObjAsync();			
	});
}
```

## How it works
An actor a associated to a string id, which is defined by the user application.
The actor will process work in a background task in the same order the work was entered to its queue.
An actor is obtained by the ActorPool, which can be the global shared instance ActorPool.Shared. or an instance of ActorPool.
Once a reference to an actor is received the user code must make sure to dispose it in order to let the library dispose it once possible.
An actor is disposed once these 2 conditions are met:
1. There are 0 references to an actor.
2. There is no work in its queue.
