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
		builder.Append('=');
		for (int i = 0; i < Values.Length; i++)
		{
			if (i > 0)
				builder.Append(", ");
			builder.Append(Values[i]);
		}
		return builder.ToString();
	}

	public readonly string Name = name;
	public readonly string[] Values = values ?? [];
}

static class BuildScript
{
	public static Entry[] Load(TextReader reader)
	=> (from line in ReadAllLines(reader) select ParseEntry(line)).ToArray();

	private static Entry ParseEntry(string line)
	{

		return new();

	}

	private static SpanRange ReadEntryKey(string line)
	{
		int start = -1;
		return new();
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

}
