using System.Collections.Generic;

namespace baco
{
	public static class TakeLastExtension
	{
		public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> xs, int count)
		{
			var q = new Queue<T>();
			foreach (var x in xs)
			{
				if (q.Count == count)
					q.Dequeue();
				q.Enqueue(x);
			}
			return q;
		}
	}
}
