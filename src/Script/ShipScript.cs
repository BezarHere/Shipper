using Shipper.TUI;
using System.Collections.Specialized;
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

	public static IEnumerable<Entry> Load(TextReader reader)
	{
		foreach (string line in ReadAllLines(reader))
		{
			string processed_line = PreprocessLine(line);

			if (processed_line.Trim().Length == 0)
				continue;
			Entry? entry_nullable = ParseEntry(processed_line);

			if (entry_nullable is not Entry entry)
				continue;

			yield return entry;
		}
	}
	public static void Dump(TextWriter writer, Entry[] entries)
	{
		foreach (Entry entry in entries)
		{
			writer.WriteLine(entry.ToString());
		}
	}

	public static string SubstituteMacros(string source, int index, Dictionary<string, string> macros)
	{
		StringBuilder builder = new(source.Length);
		int last = 0;
		foreach (IndexRange range in ApplicableRanges(source, index))
		{
			if (range.Start - last > 0)
				builder.Append(source.AsSpan(last, range.Start - last));

			string subsource = source.Substring(range);

			foreach (var (key, value) in macros)
			{
				subsource = subsource.Replace(key, value);
			}

			builder.Append(subsource);

			last = range.End;
		}
		if (last < source.Length)
			builder.Append(source.AsSpan(last, source.Length - last));
		return builder.ToString();
	}

	public static IEnumerable<IndexRange> ApplicableRanges(string source, int index)
	{
		int start = index;
		Parser.ReadingMode mode = Parser.ReadingMode.Normal;

		for (int i = index; i < source.Length; i++)
		{
			// skip escaped
			if (source[i] == '\\')
			{
				// no range should contain an escape (outside of a string) or it's escaped char
				if (mode == Parser.ReadingMode.Normal)
					yield return new IndexRange(start, i);
				// skip escaped char
				i++;
				// start after the forward slash and the escaped char
				start = i + 1;
				continue;
			}

			if (mode == Parser.ReadingMode.DoubleQuotes || mode == Parser.ReadingMode.SingleQuota)
			{
				if (source[i] == (mode == Parser.ReadingMode.DoubleQuotes ? '"' : '\''))
				{
					mode = Parser.ReadingMode.Normal;
					start = i + 1;
				}

				continue;
			}

			// Normal mode (outside strings)

			if (source[i] == '"' || source[i] == '\'')
			{
				mode = source[i] == '"' ? Parser.ReadingMode.DoubleQuotes : Parser.ReadingMode.SingleQuota;
				yield return new IndexRange(start, i);
				start = i;
				continue;
			}

			// last iteration outside a string, yield the range
			if (i == source.Length - 1)
			{
				yield return new IndexRange(start, i);
			}

		}
	}

	public static string EscapeString(string source, int start)
	{
		StringBuilder builder = new(source.Length);
		int last = 0;
		foreach (int i in GetEscapedCharIndices(source, start))
		{
			if (i - last > 0)
				builder.Append(source.AsSpan(last, i - last));
			builder.Append(source[i + 1]);
			last = i + 2;
		}
		if (last < source.Length)
			builder.Append(source.AsSpan(last, source.Length - last));
		return builder.ToString();
	}

	private static IEnumerable<int> GetEscapedCharIndices(string source, int start)
	{
		int index = start - 2;
		while (index < source.Length)
		{
			// +2 to skip current + next char (the escaped one)
			index = source.IndexOf('\\', index + 2);
			if (index == -1)
				break;
			yield return index;
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

		IEnumerable<string> values = from r in ReadEntryValues(line, index) select line.Substring(r);

		return new(line.Substring(key_range), values.ToArray());
	}

	private static IndexRange ReadEntryKey(string line, int index)
	{
		return ReadString(line, index);
	}

	private static IEnumerable<IndexRange> ReadEntryValues(string line, int index)
	{
		bool expects_separator = false;
		for (int i = index; i < line.Length; i++)
		{
			if (expects_separator)
			{
				int separator = IndexOfEntryValueSeparator(line, i);

				// reached the end of the line, stop reading
				if (separator >= line.Length)
				{
					break;
				}

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
				yield return range.Expanded(-1);
			}
			else
			{
				yield return range;
			}

			expects_separator = true;
		}

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

					// check for the case when this value is the a single character
					// at the end of the source (e.g. "bla, bla, bla, S")
					if (i == source.Length - 1)
						return new(start, start + 1);
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
			Highlight highlight = new()
			{
				Text = new(source, HighlightColor.Announcement),
				Span = new(start, start + 1)
			};


			if (mode == Parser.ReadingMode.Normal)
				highlight.Message = $"Could not read value";
			if (mode == Parser.ReadingMode.DoubleQuotes || mode == Parser.ReadingMode.SingleQuota)
				highlight.Message = $"Unclosed string";

			highlight.Message.Color = HighlightColor.Error;
			highlight.Draw();
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
