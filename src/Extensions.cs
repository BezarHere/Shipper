using Shipper.TUI;

namespace Shipper;

internal static class Extensions
{
	public static string Substring(this string str, IndexRange range) => str.Substring(range.Start, range.Length);

	public static ConsoleColor ConsoleColor(this HighlightColor color)
	{
		return color switch
		{
			HighlightColor.Debug => System.ConsoleColor.DarkGray,
			HighlightColor.Normal => System.ConsoleColor.Gray,
			HighlightColor.Announcement => System.ConsoleColor.Cyan,
			HighlightColor.Warning => System.ConsoleColor.Yellow,
			HighlightColor.Error => System.ConsoleColor.Red,
			HighlightColor.Critical => System.ConsoleColor.Magenta,
			_ => System.ConsoleColor.White,
		};
	}

	
	public static int[] IndexOfAll<T>(this T[] array, Predicate<T> predicate)
	{
		// FIXME: overkill size
		int[] results = new int[array.Length];
		int count = 0;
		for (int i = 0; i < array.Length; i++)
		{
			if (predicate(array[i]))
				results[count++] = i;
		}

		if (count < array.Length)
		{
			int[] cut = new int[count];
			Array.Copy(results, cut, count);
			return cut;
		}

		return results;
	}

}
