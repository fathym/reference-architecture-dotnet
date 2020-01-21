using Microsoft.ServiceFabric.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fathym.Fabric.Extensions
{
	public static class IAsyncEnumerableExtensions
	{
		public static async Task<T> FirstOrDefault<T>(this IAsyncEnumerable<T> values, Func<T, bool> predicate)
		{
			var enumerator = values.GetAsyncEnumerator();

			while (await enumerator.MoveNextAsync(CancellationToken.None))
				if (predicate(enumerator.Current))
					return enumerator.Current;

			return default(T);
		}

		public static async Task Each<T>(this IAsyncEnumerable<T> values, Func<T, Task> action)
		{
			await values.Each(async (value) => { await action(value); return false; });
		}

		public static async Task Each<T>(this IAsyncEnumerable<T> values, Func<T, Task<bool>> action)
		{
			if (values != null)
			{
				bool shouldBreak;

				var enumerator = values.GetAsyncEnumerator();

				while (await enumerator.MoveNextAsync(CancellationToken.None))
				{
					shouldBreak = await action(enumerator.Current);

					if (shouldBreak)
						break;
				}
			}
		}

		public static async Task<T> LastOrDefault<T>(this IAsyncEnumerable<T> values, Func<T, bool> predicate)
		{
			var value = default(T);

			var enumerator = values.GetAsyncEnumerator();

			while (await enumerator.MoveNextAsync(CancellationToken.None))
				if (predicate(enumerator.Current))
					value = enumerator.Current;

			return value;
		}

		public static async Task<bool> IsNullOrEmpty<T>(this IAsyncEnumerable<T> values)
		{
			if (values == null)
				return true;

			var enumerator = values.GetAsyncEnumerator();

			while (await enumerator.MoveNextAsync(CancellationToken.None))
				return false;

			return true;
		}

		public static async Task<IEnumerable<TTo>> Select<T, TTo>(this IAsyncEnumerable<T> values, Func<T, TTo> action)
		{
			var selected = new List<TTo>();

			var enumerator = values.GetAsyncEnumerator();

			while (await enumerator.MoveNextAsync(CancellationToken.None))
				selected.Add(action(enumerator.Current));

			return selected;
		}

		public static async Task<IEnumerable<T>> Where<T>(this IAsyncEnumerable<T> values, Func<T, bool> predicate)
		{
			var matches = new List<T>();

			var enumerator = values.GetAsyncEnumerator();

			while (await enumerator.MoveNextAsync(CancellationToken.None))
				if (predicate(enumerator.Current))
					matches.Add(enumerator.Current);

			return matches;
		}
	}
}
