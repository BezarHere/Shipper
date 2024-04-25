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

	private enum ReadingMode
	{
		Normal,
		DoubleQuotes,
		SingleQuota
	}

	private static Range ReadCommandlet(string source, int index)
	{
		int start = -1;
		ReadingMode mode = 0;
		for (int i = index; i < source.Length; i++)
		{
			if (start > -1)
			{
				if (mode == ReadingMode.Normal && char.IsWhiteSpace(source[i]))
				{
					return new(start, i);
				}

				if (mode == ReadingMode.DoubleQuotes && source[i] == '"')
				{
					
				}



				continue;
			}

			if (!char.IsWhiteSpace(source[i]))
			{
				if (source[i] == '"' || source[i] == '\'')
				{
					reading = source[i];
				}
				else
				{
					reading = 1;
				}
			}
		}
		return range;
	}

	private static Range[] SplitCommandlets()
	{
		return [];
	}

	public static Argument[] ParseArguments(string args)
	{


		return [];
	}

	public readonly string Name;
	public List<string> Uses;

	public readonly bool HasBeenRead { get => Uses.Count > 0; }

}


