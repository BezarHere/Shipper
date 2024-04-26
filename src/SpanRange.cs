namespace Shipper;
readonly struct SpanRange(int start, int end)
{
	public readonly int Start = start;
	public readonly int End = end;

	public readonly bool Contains(int index)
	{
		return index >= Start && index < End;
	}

	public static implicit operator Range(SpanRange range)
	{
		return new(range.Start, range.End);
	}

	public static explicit operator int(SpanRange range)
	{
		return range.Length;
	}
	public readonly int Length { get => End - Start; }

	/// <summary>
	/// valid ranges have non-zero, positive length
	/// </summary>
	public readonly bool Valid { get => Length > 0; }
}
