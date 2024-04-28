using System.Text;

namespace Shipper;
readonly internal struct LineInput(string source, Argument[] arguments)
{
	public readonly string Source = source;
	public readonly Argument[] Arguments = arguments;

	public static LineInput FromLine(string line)
	{
		return new(line, Argument.ParseArguments(line));
	}

	public static LineInput FromArgs(string[] args)
	{
		Argument[] arguments = new Argument[args.Length];
		int count = 0;
		StringBuilder builder = new(args.Length * 10);

		foreach (string arg in args)
		{
			int builder_start_ln = builder.Length;
			if (arg.Any(char.IsWhiteSpace))
			{
				builder.Append('"').Append(arg).Append('"');
			}
			else
			{
				builder.Append(arg);
			}

			arguments[count++] = new(arg, builder_start_ln..builder.Length);
			builder.Append(' ');
		}

		return new(builder.ToString().TrimEnd(), arguments);
	}

}
