using Shipper.Commands;
using Shipper.Script;

namespace Shipper;
struct Argument
{
	public Argument(string content, IndexRange? span = null)
	{
		Content = content;
		Span = span ?? default;
		Uses = [];
	}

	public override readonly string ToString()
	{
		return Content;
	}

	public readonly string Use(ICommand command) => Use(command.Name);

	public readonly string Use(string use)
	{
		Uses.Add(use);
		return Content;
	}

	public readonly string Content;
	public readonly IndexRange Span;
	public List<string> Uses;

	public readonly bool HasBeenUsed { get => Uses.Count > 0; }

	

	private static IndexRange ReadCommandlet(string source, int index)
	{
		return Parser.ReadIdentifier(source, index);
	}

	private static IndexRange[] SplitCommandlets(string source)
	{
		List<IndexRange> ranges = new();
		int index = 0;
		while (index < source.Length)
		{
			IndexRange range = ReadCommandlet(source, index);
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
		IndexRange[] commandlets = SplitCommandlets(args);
		Argument[] arguments = new Argument[commandlets.Length];

		for (int i = 0; i < commandlets.Length; i++)
		{
			IndexRange range = commandlets[i];
			// if the commandlet starts and ends with quotes, trim the quotes (e.g. `"hello there"` -> `hello there`)
			if (args[range.Start] == args[range.End - 1] && (args[range.Start] == '"' || args[range.Start] == '\''))
			{
				range = new(range.Start + 1, range.End - 1);
			}

			arguments[i] = new(args.Substring(range.Start, range.Length), range);
		}

		return arguments;
	}

}


