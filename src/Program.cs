
using Shipper.Commands;
using Shipper.TUI;
using System.Text;

namespace Shipper;

internal class Program
{
	public static Error RunInteractive()
	{
		Console.WriteLine("Starting the interactive terminal:");

		RunningInteractive = true;
		Error result = Error.Ok;

		while (RunningInteractive)
		{
			Console.Write(">> ");
			string line = (Console.ReadLine() ?? "quit").Trim();
			if (line == "quit" || line == "exit" || line == "q")
			{
				RunningInteractive = false;
				break;
			}

			Argument[] arguments = Argument.ParseArguments(line);
			Error error = Run(new(line, arguments));

			// non-zero result indicate an error
			//if (error != Error.Ok)
			//{
			//	result = error;
			//	break;
			//}
		}



		return result;
	}

	private static Error Run(LineInput input)
	{
		var arguments = input.Arguments;
		ICommand? command = Environment.GetCommand(arguments[0].Name);

		if (command is null)
		{
			Console.WriteLine($"Unknown command: '{arguments[0].Name}'");


			return Error.UnknownCommand;
		}

		arguments[0].Use("command");

		Argument[] passed_arguments = new Argument[arguments.Length - 1];
		Array.Copy(arguments, 1, passed_arguments, 0, passed_arguments.Length);
		Error result = command.Execute(passed_arguments);

		if (result != Error.Ok)
		{
			Console.WriteLine($"Command '{command.Name}' has returned an error: {ErrorUtility.GetName(result)}");
			return result;
		}

		for (int i = 0; i < arguments.Length; i++)
		{
			if (!arguments[i].HasBeenUsed)
			{
				Highlight highlight = new()
				{
					Text = input.Source,
					Message = $"Unknown argument No.{i}",
					Range = arguments[i].Span,
				};
				highlight.Draw(Console.Out);
			}
		}

		return Error.Ok;
	}

	private static LineInput MarshalArgs(string[] args)
	{
		Argument[] arguments = new Argument[args.Length];
		int position = 0;

		for (int i = 0; i < args.Length; i++)
		{
			int length = args[i].Length + (args[i].Contains(' ') ? 2 : 0);
			arguments[i] = new(args[i], new SpanRange(position, length));
			position += length + 1;
		}

		StringBuilder source = new();
		foreach (string arg in args)
		{
			if (arg.Contains(' '))
			{
				source.Append('"');
				source.Append(arg);
				source.Append('"');
			}
			else
			{
				source.Append(arg);
			}

			source.Append(' ');
		}

		return new(source.ToString().TrimEnd(), arguments);
	}

	public static int Main(string[] args)
	{
		//# Environment is initialized in it's ctor

		if (args.Length == 0)
			return (int)RunInteractive();

		LineInput input = MarshalArgs(args);
		return (int)Run(input);
	}

	public static Environment Environment { get; private set; } = new();
	public static bool RunningInteractive { get; private set; } = false;
}
