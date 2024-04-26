using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shipper;
struct Argument
{
	public Argument(string name)
	{
		Name = name;
		Uses = [];
	}

	public override string ToString()
	{
		return Name;
	}

	private enum ReadingMode
	{
		Normal,
		DoubleQuotes,
		SingleQuota
	}

	private static SpanRange ReadCommandlet(string source, int index)
	{
		int start = -1;
		ReadingMode mode = 0;
		for (int i = index; i < source.Length; i++)
		{
			if (start > -1)
			{
				if (mode == ReadingMode.Normal)
				{
					if (char.IsWhiteSpace(source[i]))
						return new(start, i);

					if (source[i] == '"')
					{
						mode = ReadingMode.DoubleQuotes;
					}
					else if (source[i] == '\'')
					{
						mode = ReadingMode.SingleQuota;
					}

					continue;
				}

				if ((mode == ReadingMode.DoubleQuotes && source[i] == '"') | (mode == ReadingMode.SingleQuota && source[i] == '\''))
				{
					mode = ReadingMode.Normal;
				}

				

				continue;
			}

			if (!char.IsWhiteSpace(source[i]))
			{
				start = i;

				if (source[i] == '"')
				{
					mode = ReadingMode.DoubleQuotes;
				}
				else if (source[i] == '\'')
				{
					mode = ReadingMode.SingleQuota;
				}
				else
				{
					mode = ReadingMode.Normal;
				}
			}
		}

		if (start < index)
		{
			return new(0, 0);
		}

		return new(start, source.Length);
	}

	private static SpanRange[] SplitCommandlets(string source)
	{
		List<SpanRange> ranges = new();
		int index = 0;
		while (index < source.Length)
		{
			SpanRange range = ReadCommandlet(source, index);
			if (!range.Valid)
			{
				break;
			}

			ranges.Add(range);

			index = range.End;
		}
		return ranges.ToArray();
	}

	public static Argument[] ParseArguments(string args)
	{
		SpanRange[] commandlets = SplitCommandlets(args);
		Argument[] arguments = new Argument[commandlets.Length];

		for (int i = 0; i < commandlets.Length; i++)
		{
			SpanRange range = commandlets[i];
			// if the commandlet starts and ends with quotes, trim the quotes (e.g. `"hello there"` -> `hello there`)
			if (args[range.Start] == args[range.End - 1] && (args[range.Start] == '"' || args[range.Start] == '\''))
			{
				range = new(range.Start + 1, range.End - 1);
			}

			arguments[i] = new(args.Substring(range.Start, range.Length));
		}

		return arguments;
	}

	public readonly string Name;
	public List<string> Uses;

	public readonly bool HasBeenRead { get => Uses.Count > 0; }

}


