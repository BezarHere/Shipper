
using Shipper.Commands;
using Shipper.Script;
using Shipper.TUI;

namespace Shipper;

internal class Program
{

	public static Error RunInteractive()
	{
		Console.WriteLine("Starting the interactive terminal:");
		SetupInteractive();

		Error result = Error.Ok;

		while (RunningInteractive)
		{
			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.Write(">> ");
			Console.ForegroundColor = ConsoleColor.Gray;
			string line = (Console.ReadLine() ?? "quit").Trim();
			if (line == "quit" || line == "exit" || line == "q")
			{
				RunningInteractive = false;
				break;
			}

			Error error = Run(LineInput.FromLine(line));

			// non-zero result indicate an error
			//if (error != Error.Ok)
			//{
			//	result = error;
			//	break;
			//}
		}



		return result;
	}

	private static Error RunStartup(string[] args)
	{
		foreach (string[] sub_args in SeparateMainArgs(args))
		{
			Error err = Run(LineInput.FromArgs(sub_args));
			if (err != Error.Ok)
				return err;
		}
		return Error.Ok;
	}

	private static Error Run(LineInput input)
	{
		var arguments = input.Arguments;
		if (arguments.Length == 0)
		{
			return Error.Ok;
		}

		ICommand? command = ShipperCore.GetCommand(arguments[0].Content);

		if (command is null)
		{
			Console.WriteLine($"Unknown command: '{arguments[0].Content}'");
			return Error.UnknownCommand;
		}

		arguments[0].Use("command");

		Argument[] passed_arguments = new Argument[arguments.Length - 1];
		Array.Copy(arguments, 1, passed_arguments, 0, passed_arguments.Length);

		Error result = command.Execute(
			passed_arguments,
			RunningInteractive ? CommandCallContext.InteractiveTerminal : CommandCallContext.ProgramStartup
			);

		if (result != Error.Ok)
		{
			Console.ForegroundColor = ConsoleColor.DarkMagenta;
			Console.WriteLine($"Command '{command.Name}' has returned an error: {ErrorUtility.GetName(result)}");
			Console.ForegroundColor = ConsoleColor.Gray;
		}

		for (int i = 0; i < arguments.Length; i++)
		{
			if (!arguments[i].HasBeenUsed)
			{
				Highlight highlight = new()
				{
					Text = new(input.Source, HighlightColor.Announcement),
					Message = new($"Unknown argument No.{i}", HighlightColor.Warning),
					Span = arguments[i].Span,
				};
				highlight.Draw(Console.Out);
			}
		}

		return result;
	}

	private static int Main(string[] args)
	{
		ShipperCore.Init(ref args);

		Glob glob = new("**/*.cs");
		string test = "F:\\Assets\\visual studio\\Shipper\\project_demo.ship";

		if (args.Length == 0)
			return (int)RunInteractive();

		return (int)RunStartup(args);
	}

	private static void SetupInteractive()
	{
		RunningInteractive = true;

		Console.Title = "Shipper";
		if (DateTime.Now.Day == 1 && DateTime.Now.Month == 4)
			Console.Title = "Shipper: [INSERT APRIL JOKE]";
	}

	private static IEnumerable<string[]> SeparateMainArgs(string[] args)
	{
		int start = 0;
		for (int i = 0; i < args.Length; i++)
		{
			if (args[i] == "+")
			{
				yield return args[start..i];
				start = i + 1;
			}
			else if (args[i].Length > 0 && args[i][0] == '+')
			{
				yield return args[start..i];
				args[i] = args[i][1..];
				start = i;
			}
		}
		yield return args[start..];
	}

	public static bool RunningInteractive { get; private set; } = false;
}
