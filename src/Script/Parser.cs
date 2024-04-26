using System;

namespace Shipper.Script;

static class Parser
{
	public enum ReadingMode
	{
		Normal,
		DoubleQuotes,
		SingleQuota
	}

	public static SpanRange ReadIdentifier(string source, int index)
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
}
