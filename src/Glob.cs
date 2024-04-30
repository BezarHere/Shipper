namespace Shipper;

internal struct Glob(string source)
{
	private enum SegmentType
	{
		Invalid = -1,
		Text,
		DirectorySeparator,
		CharSelect, // select chars '[CcBb]all.json'
		CharSelectNot, // select any other chars 'non-vowel_[!aeuioy].doc'
		AnyChar, // ?
		AnyName, // star
		AnyPath, // double star
	}

	private delegate int GreedySegmentTester(string path, IndexRange range);

	private readonly struct Segment(SegmentType type, IndexRange? range = null)
	{
		public Segment(SegmentType type, char[] selected, IndexRange range) : this(type, range)
		{
			Selected = selected;
		}

		public const int MaxSelectedChars = 64;
		public readonly SegmentType Type = type;
		public readonly IndexRange Range = range ?? new();
		public readonly char[] Selected = [];

		public int Length
		{
			get
			{
				return Type switch
				{
					SegmentType.Text or SegmentType.AnyChar => Range.Length,
					SegmentType.CharSelect or SegmentType.CharSelectNot or SegmentType.DirectorySeparator => 1,
					SegmentType.AnyName or SegmentType.AnyPath => int.MaxValue,
					_ => 0,
				};
			}
		}

		public bool Greedy
		{
			get
			{
				if (Type == SegmentType.AnyName || Type == SegmentType.AnyPath)
					return true;
				return false;
			}
		}
	}

	/// <summary>
	/// tests if the path is matched by this glob
	/// </summary>
	/// <param name="path">the path to be tested</param>
	/// <param name="strict">sets strict mode, making it a requirement to test the entire path</param>
	/// <returns>if the glob matches the path</returns>
	public readonly bool Test(string path, bool strict = false)
	{
		int index = 0;
		Span<int> skip_table = stackalloc int[segments.Length];

		for (int i = 0; i < segments.Length; i++)
		{
			// TODO: handle AnyName/AnyPath segment's greediness
			int result = TestSegment(i, path, index, skip_table[i]);

			// segment failed
			if (result == 0)
			{
				return false;
			}

			index += result;

			// finished the path: if there are any segments untested, return false otherwise true
			if (index >= path.Length)
				return i == segments.Length - 1;
		}

		// strict makes it a requirement to finish the path/string
		if (strict)
			return index == segments.Length - 1;

		return true;
	}

	private readonly (int pos, int len) TestWalkSegment(int segment_index, string path, int start_index, int skip = 0)
	{
		if (segments[segment_index].Greedy)
			return (-1, 0);

		int walk_end = path.Length - segments[segment_index].Length;
		for (int i = start_index; i <= walk_end; i++)
		{
			int result = TestSegment(segment_index, path, i, 0);
			if (result != 0)
			{
				if (skip <= 0)
					return (i, result);

				skip--;
			}
		}
		return (-1, 0);
	}

	private readonly (int pos, int len) TestWalkSegmentReversed(int segment_index, string path, int start_index, int skip = 0)
	{
		if (segments[segment_index].Greedy)
			return (-1, 0);

		int walk_end = path.Length - segments[segment_index].Length;
		for (int i = walk_end; i >= start_index; i--)
		{
			int result = TestSegment(segment_index, path, i, 0);
			if (result != 0)
			{
				if (skip <= 0)
					return (i, result);

				skip--;
			}
		}
		return (-1, 0);
	}


	/// <returns>the length read</returns>
	private static int TestSegmentAnyName(string path, IndexRange range)
	{
		foreach (int i in range)
		{
			if (path[i].IsDirectorySeparator())
			{
				return i - range.Start;
			}
		}
		return range.Length;
	}

	/// <returns>the length read</returns>
	private static int TestSegmentAnyPath(string path, IndexRange range)
	{
		// to stop warnings, TODO: find a 'MaybeUnused'-like attribute
		_ = path;

		// any path is just any name but matching the directory separator
		// so, it's match everything
		return range.Length;
	}

	private readonly int TestSegment(int segment_index, string path, int index, int skip = 0)
	{
		var segment = segments[segment_index];

		switch (segment.Type)
		{
			case SegmentType.Text:
			{
				for (int i = 0; i < segment.Range.Length; i++)
				{
					if (path[index + i] != Source[segment.Range.Start + i])
						return 0;
				}
				return segment.Range.Length;
			}

			case SegmentType.DirectorySeparator:
			{

				for (int i = index; i < path.Length; i++)
				{
					if (!(path[i] == '\\' || path[i] == '/'))
						return i - index;
				}
				return path.Length - index;
			}

			case SegmentType.CharSelect:
			{
				if (segment.Selected.Contains(path[index]))
					return 1;
				return 0;
			}

			case SegmentType.CharSelectNot:
			{
				if (segment.Selected.Contains(path[index]))
					return 0;
				return 1;
			}

			case SegmentType.AnyChar:
			{
				// matches n-many chars
				//! NOTE: AnyChar groups all consecutive '?' to one segment
				if (path.Length - index < segment.Length)
					return 0;
				return segment.Length;
			}

			// read until the next segment (if any) is satisfied
			case SegmentType.AnyName:
			case SegmentType.AnyPath:
			{
				GreedySegmentTester tester = segment.Type == SegmentType.AnyName ? TestSegmentAnyName : TestSegmentAnyPath;

				if (segment_index == segments.Length - 1 || segments[segment_index].Length == 0)
				{
					return tester(path, index..path.Length);
				}

				var (pos, result) = TestWalkSegmentReversed(segment_index + 1, path, index, skip);

				if (pos == -1 || result == 0)
				{
					// no need to return an error, next segment will fail
					return tester(path, index..path.Length);
				}

				return tester(path, index..pos);
			}

			default:
				return 0;
		}
	}

	public string Source { get; init; } = source;
	private readonly Segment[] segments = Parse(source);

	private static Segment[] Parse(string source)
	{
		List<Segment> list = new();

		for (int i = 0; i < source.Length; i++)
		{
			Segment segment = ParseSegment(source, i);
			if (segment.Type == SegmentType.Invalid || segment.Range.Length < 1)
				continue;

			i += segment.Range.Length - 1;

			list.Add(segment);
		}

		return list.ToArray();
	}

	private static Segment ParseSegment(string source, int index)
	{
		if (source[index] == '*')
		{

			if (index < source.Length - 1 && source[index + 1] == '*')
				return new(SegmentType.AnyPath, new IndexRange(index, index + 2));
			return new(SegmentType.AnyName, new IndexRange(index, index + 1));
		}

		if (source[index] == '[')
		{
			return ParseSelector(source, index + 1);
		}

		if (source[index] == '\\' || source[index] == '/')
		{
			int count = source.AsSpan().CountContinues(c => c == '/' || c == '\\', index);
			return new(SegmentType.DirectorySeparator, new IndexRange(index, index + count));
		}

		if (source[index] == '?')
		{
			int count = source.AsSpan().CountContinues(c => c == '?', index);
			return new(SegmentType.AnyChar, new IndexRange(index, index + count));
		}

		return ParseText(source, index);
	}

	private static Segment ParseText(string source, int index)
	{
		for (int i = index; i < source.Length; i++)
		{
			if (source[i] == '[' || source[i] == '*' || source[i] == '/' || source[i] == '\\')
				return new(SegmentType.Text, index..i);
		}

		return new(SegmentType.Text, index..source.Length);
	}

	private static Segment ParseSelector(string source, int index)
	{
		bool inverted = source[index] == '!';
		int start = index;
		if (inverted)
			start++;

		int count = 0; // count of chars selected
		int count_raw = 0; // count of 
		Span<char> selected = stackalloc char[Segment.MaxSelectedChars];

		int source_ln = source.Length;

		for (int i = start; i < source_ln; i++)
		{
			count_raw++;

			if (count == Segment.MaxSelectedChars)
			{
				if (source[i] != ']')
					throw new ParseError($"Selected too many characters, Max is {Segment.MaxSelectedChars}", index..i, source);
				break;
			}

			if (source[i] == '\\')
			{
				// escape just before the end; we need to read a terminating ']'
				if (i >= source_ln - 2)
					break;
				selected[count++] = source[++i];
				continue;
			}

			// char is a termination, no need to continue
			if (source[i] == ']')
				break;

			selected[count++] = source[i];

			// last index, didn't find a terminating ']'
			if (i == source_ln - 1)
				throw new ParseError("Expecting a termination ']'", (index..i), source);
		}

		// +1 for the terminating ']'; if ']' is not read, then it will turn to a text segment
		count_raw++;

		return new(
				inverted ? SegmentType.CharSelectNot : SegmentType.CharSelect,
				selected.ToArray(),
				new(index, start + count_raw)
			);
	}
}
