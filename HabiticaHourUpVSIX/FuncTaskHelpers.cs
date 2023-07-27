using System;
using System.Collections.Generic;
using System.Threading.Tasks;
#nullable enable

namespace Ar6yZuK.MethodHelpers;
public static class FuncTaskHelpers
{
	public static Task InvokeMultipleAsync<T>(this Func<T, Task>? obj1, T argument)
	{
		if (obj1 is null)
			return Task.CompletedTask;

		var subscribers = obj1.GetInvocationList();
		if(subscribers.Length is 0)
			return Task.CompletedTask;

		return InvokeMultipleInternalAsync(subscribers, argument);
		static Task InvokeMultipleInternalAsync(Delegate[] subscribers, T argument)
		{
			var tasks = new List<Task>(subscribers.Length);

			foreach (Func<T, Task> subscriber in subscribers)
			{
				var task = subscriber.Invoke(argument);
				tasks.Add(task);
			}

			return Task.WhenAll(tasks);
		}
	}

	// Must invoke all combined delegates
	public static Task InvokeMultipleAsync(this Func<Task>? obj1)
	{
		if (obj1 is null)
			return Task.CompletedTask;

		var subscribers = obj1.GetInvocationList();
		if (subscribers.Length is 0)
			return Task.CompletedTask;

		return InvokeMultipleInternalAsync(subscribers);
		static Task InvokeMultipleInternalAsync(Delegate[] subscribers)
		{
			var tasks = new List<Task>(subscribers.Length);

			foreach (Func<Task> subscriber in subscribers)
			{
				var task = subscriber.Invoke();
				tasks.Add(task);
			}

			return Task.WhenAll(tasks);
		}
	}
}
