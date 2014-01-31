using System.Text.RegularExpressions;

namespace baco
{
	public static class Filter
	{
		public static bool Where(string input, Regex include, Regex exclude)
		{
			return (include == null || include.IsMatch(input)) && (exclude == null || !exclude.IsMatch(input));
		}
	}
}
