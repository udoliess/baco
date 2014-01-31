using System.Collections.Generic;

namespace baco
{
	public static class WithoutLastExtension
	{
		public static IEnumerable<T> WithoutLast<T>(this IEnumerable<T> xs, int count)
		{
			var q = new Queue<T>();
			foreach (var x in xs)
			{
				if (q.Count == count)
					yield return q.Dequeue();
				q.Enqueue(x);
			}
		}
	}
}
