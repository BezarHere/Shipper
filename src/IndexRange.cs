using System.Collections;

namespace Shipper;
readonly struct IndexRange(int start, int end) : IEnumerable<int>
{
	public readonly int Start = start;
	public readonly int End = end;
	public readonly int Length { get => End - Start; }

	public IndexRange() : this(-1, -1) { }

	/// <summary>
	/// valid ranges have non-zero, positive length
	/// </summary>
	public readonly bool Valid { get => Length > 0; }

	public readonly bool Contains(int index)
	{
		return index >= Start && index < End;
	}

	public readonly IndexRange Expanded(int offset)
	{
		return new(Start - offset, End + offset);
	}

	public IEnumerator<int> GetEnumerator()
	{
		for (int i = Start; i < End; i++) yield return i;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public static implicit operator Range(IndexRange range)
	{
		return new(range.Start, range.End);
	}

	public static implicit operator IndexRange(Range range)
	{
		return new(range.Start.Value, range.End.Value);
	}

	public static explicit operator int(IndexRange range)
	{
		return range.Length;
	}

}
