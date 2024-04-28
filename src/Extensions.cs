using Shipper.TUI;
using System.Numerics;

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

	/// <summary>
	/// evaluates all the elements using a transformer function
	/// </summary>
	/// <typeparam name="T">the array type</typeparam>
	/// <param name="array">array to be find the most evaluated element</param>
	/// <param name="evaluator">a transformer from an element to it's score</param>
	/// <returns>the index of the element with the highest score, or -1 if the array is empty</returns>
	public static int Evaluate<T>(this T[] array, Func<T, int> evaluator)
	{
		int score = int.MinValue;
		int index = -1;

		for (int i = 0; i < array.Length; i++)
		{
			int evl = evaluator(array[i]);
			if (evl > score)
			{
				score = evl;
				index = i;
			}
		}

		return index;
	}

	/// <summary>
	/// counts all contiguous elements satisfying the predicate, stops when the predicate fails
	/// </summary>
	/// <typeparam name="T">the array type</typeparam>
	/// <param name="collection">the array</param>
	/// <param name="start">the counting start position</param>
	/// <param name="predicate">the counter predicate</param>
	/// <returns>how many elements where counted</returns>
	public static int CountContinues<T>(this T[] collection, Predicate<T> predicate, int start = 0)
	{
		for (int i = start; i < collection.Length; i++)
		{
			if (!predicate(collection[i]))
				return i - start;
		}
		return collection.Length - start;
	}

	/// <summary>
	/// counts all contiguous elements satisfying the predicate, stops when the predicate fails
	/// </summary>
	/// <typeparam name="T">the collection type</typeparam>
	/// <param name="collection">the collection</param>
	/// <param name="start">the counting start position</param>
	/// <param name="predicate">the counter predicate</param>
	/// <returns>how many elements where counted</returns>
	public static int CountContinues<T>(this T collection, Predicate<T> predicate, int start = 0) where T : ICollection<T>, IList<T>
	{
		for (int i = start; i < collection.Count; i++)
		{
			if (!predicate(collection[i]))
				return i - start;
		}
		return collection.Count - start;
	}

	public static T ToKilobyte<T>(this T value) where T : IShiftOperators<T, int, T>, INumber<T>
	{
		return value << 10;
	}

	public static T ToMegabyte<T>(this T value) where T : IShiftOperators<T, int, T>, INumber<T>
	{
		return value << 20;
	}

	public static T AsKilobyte<T>(this T value) where T : IShiftOperators<T, int, T>, INumber<T>
	{
		return value >> 10;
	}

	public static T AsMegabyte<T>(this T value) where T : IShiftOperators<T, int, T>, INumber<T>
	{
		return value >> 20;
	}

}