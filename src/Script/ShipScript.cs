using Shipper.TUI;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace Shipper.Script;

readonly struct Entry(string name, string[]? values = null)
{
	public override int GetHashCode()
	{
		return Name.GetHashCode() ^ Values.GetHashCode();
	}

	public override string ToString()
	{
		StringBuilder builder = new();
		builder.Append(Name);
		builder.Append(" =");
		if (Values.Length > 0)
			builder.Append(' ');

		for (int i = 0; i < Values.Length; i++)
		{
			if (i > 0)
				builder.Append(", ");
			builder.Append('"');
			builder.Append(Values[i]);
			builder.Append('"');
		}
		return builder.ToString();
	}

	public readonly string Name = name;
	public readonly string[] Values = values ?? [];
}

static class ShipScript
{

	public static Entry[] Load(TextReader reader)
	{
		List<Entry> entries = new();
		foreach (string line in ReadAllLines(reader))
		{
			string processed_line = PreprocessLine(line);

			if (processed_line.Trim().Length == 0)
				continue;
			Entry? entry_nullable = ParseEntry(processed_line);

			if (entry_nullable is not Entry entry)
				continue;

			entries.Add(entry);
		}
		return entries.ToArray();
	}
	public static void Dump(TextWriter writer, Entry[] entries)
	{
		foreach (Entry entry in entries)
		{
			writer.WriteLine(entry.ToString());
		}
	}

	private static string PreprocessLine(string line)
	{
		int quotes = 0;
		int double_quotes = 0;
		for (int i = 0; i < line.Length; i++)
		{
			if (line[i] == '"')
			{
				double_quotes++;
				continue;
			}

			if (line[i] == '\'')
			{
				quotes++;
				continue;
			}

			// comment prefix and no unclosed quotes/double quotes
			if (line[i] == CommentPrefix && ((quotes | double_quotes) & 1) == 0)
			{
				line = line[..i];
				break;
			}

		}

		return line;
	}

	private static Entry? ParseEntry(string line)
	{
		int index = 0;
		IndexRange key_range = ReadEntryKey(line, index);
		if (!key_range.Valid)
		{
			Console.WriteLine($"Couldn't read the key in '{line}'");
			return null;
		}
		index = key_range.End;

		{
			int equal_sign = line.IndexOf('=', index);
			if (equal_sign == -1)
			{
				Console.WriteLine($"Couldn't find an equal sign in '{line}'");
				return null;
			}

			index = equal_sign + 1;
		}

		IndexRange[] ranges = ReadEntryValues(line, index);
		string[] values = new string[ranges.Length];
		for (int i = 0; i < ranges.Length; i++)
		{
			values[i] = line.Substring(ranges[i]);
		}

		return new(line.Substring(key_range), values);
	}

	private static IndexRange ReadEntryKey(string line, int index)
	{
		return ReadString(line, index);
	}

	private static IndexRange[] ReadEntryValues(string line, int index)
	{
		List<IndexRange> ranges = new();
		bool expects_separator = false;
		for (int i = index; i < line.Length; i++)
		{
			if (expects_separator)
			{
				int separator = IndexOfEntryValueSeparator(line, i);
				if (separator < i)
				{
					Highlight highlight = new()
					{
						Text = line,
						Message = "Expected ',' to separate values",
						// the negative separator might encode where we found the unexpected character
						Span = separator < -i ? new(-separator, -separator + 1) : new(i, i + 1)
					};
					highlight.Draw();
					break;
				}

				// no more values or read the line completely
				if (char.IsWhiteSpace(line[separator]) || separator >= line.Length)
					break;

				// something isn't right if this is true
				if (line[separator] != ValueSeparator)
				{
					Highlight highlight = new()
					{
						Text = line,
						Message = $"Unexpected '{line[separator]}'",
						Span = new(separator, separator + 1)
					};
					highlight.Draw();
					break;
				}


				// found a separator, set cursor to what's after it
				i = separator + 1;
				expects_separator = false;
			}


			IndexRange range = ReadString(line, i);
			if (!range.Valid)
				break;
			i = range.End - 1;

			// check & trim quotes
			if (line[range.Start] == line[range.End - 1] && (line[range.Start] == '\'' || line[range.Start] == '"'))
			{
				ranges.Add(range.Expanded(-1));
			}
			else
			{
				ranges.Add(range);
			}

			expects_separator = true;
		}

		return ranges.ToArray();
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="line"></param>
	/// <param name="index"></param>
	/// <returns>the index of the next comma or next new line for last values</returns>
	private static int IndexOfEntryValueSeparator(string line, int index)
	{
		for (int i = index; i < line.Length; i++)
		{

			// skip escaped shit
			if (line[i] == '\\' && char.IsWhiteSpace(line[i + 1]))
			{
				i++;
				continue;
			}

			if (line[i] == '\n')
				return i;
			if (line[i] == ValueSeparator)
				return i;

			if (!char.IsWhiteSpace(line[i]))
				return -i;

			// if we are the end of the line/string
			if (i == line.Length - 1)
				return line.Length;
		}
		return -1;
	}

	private static int FindClosingQuote(string source, char quote, int index)
	{
		for (int i = 0; i < source.Length; i++)
		{
			if (source[i] == '\\')
			{
				i++;
				continue;
			}

			if (source[i] == quote)
				return i;
		}
		return -1;
	}

	private static bool IsOperatorChar(char c)
	=> c == ValueSeparator || c == AssignmentOperator;

	private static IndexRange ReadString(string source, int index)
	{
		int start = -1;
		Parser.ReadingMode mode = Parser.ReadingMode.Normal;

		for (int i = index; i < source.Length; i++)
		{
			if (source[i] == '\\')
			{
				// skip current and next char
				i++;
				continue;
			}

			// didn't start reading
			if (start < 0)
			{
				if (char.IsWhiteSpace(source[i]))
					continue;

				if (IsOperatorChar(source[i]))
					break;

				start = i;

				if (source[i] == '"')
				{
					mode = Parser.ReadingMode.DoubleQuotes;
				}
				else if (source[i] == '\'')
				{
					mode = Parser.ReadingMode.DoubleQuotes;
				}
				else
				{
					mode = Parser.ReadingMode.Normal;
				}


				continue;
			}

			if (mode == Parser.ReadingMode.Normal)
			{
				// is the char a whitespace or an operator
				if (char.IsWhiteSpace(source[i]) || IsOperatorChar(source[i]))
					return new(start, i);

				// it's all a string, 'til the end
				if (source.Length - 1 == i)
					return new(start, source.Length);
				continue;
			}

			if (mode == Parser.ReadingMode.SingleQuota)
			{
				if (source[i] == '\'')
					return new(start, i + 1);
				continue;
			}

			if (mode == Parser.ReadingMode.DoubleQuotes)
			{
				if (source[i] == '"')
					return new(start, i + 1);
				continue;
			}

		}

		// has set a start but didn't complete string reading?
		if (start >= 0)
		{
			if (mode == Parser.ReadingMode.Normal)
				Console.WriteLine($"Could not read string at [{start}] from source: \"{source}\"");
			if (mode == Parser.ReadingMode.DoubleQuotes || mode == Parser.ReadingMode.SingleQuota)
				Console.WriteLine($"Unclosed string at [{start}] in source: \"{source}\"");
		}

		return default;
	}

	private static string[] ReadAllLines(TextReader reader)
	{
		List<string> lines = new();
		while (reader.Peek() != -1)
		{
			// trimming removes whitespace, also erases only-whitespace strings
			string line = ReadLine(reader).Trim();

			if (line.Length == 0)
				continue;

			lines.Add(line);
		}

		return lines.ToArray();
	}

	// takes escaped new lines to account
	private static string ReadLine(TextReader reader)
	{
		// most lines are not escaped, no need to use the string builder
		string result = "";


		while (reader.Peek() != -1)
		{
			string? line = reader.ReadLine();
			if (line is null)
				break;

			line = line.Trim();
			result += line;

			if (line.EndsWith('\\'))
			{
				result += ' ';
				continue;
			}
			break;
		}

		return result;
	}

	private const char CommentPrefix = '#';
	private const char ValueSeparator = ',';
	private const char AssignmentOperator = '=';
}
