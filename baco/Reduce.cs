using System;
using System.Globalization;

namespace baco
{
	/// <summary>
	/// Reduce.
	/// This class is compareable by age component.
	/// </summary>
	public class Reduce : IComparable<Reduce>
	{
		/// <summary>
		/// Initializes a new instance of the Reduce class.
		/// </summary>
		/// <param name='age'>
		/// Age.
		/// </param>
		/// <param name='span'>
		/// Span.
		/// </param>
		public Reduce(string age, string span)
		{
			this.age = TimeSpan.Parse(age, CultureInfo.InvariantCulture);
			this.span = TimeSpan.Parse(span, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Initializes a new instance of the Reduce class for comparing/searching only.
		/// </summary>
		/// <param name='age'>
		/// Age.
		/// </param>
		public Reduce(TimeSpan age)
		{
			this.age = age;
		}

		TimeSpan age;
		TimeSpan? span;

		public TimeSpan Age { get { return age; } }

		public TimeSpan Span { get { return span.Value; } }

		public int CompareTo(Reduce other)
		{
			return age.CompareTo(other.age);
		}
	}
}

