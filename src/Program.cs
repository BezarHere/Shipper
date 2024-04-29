
using Shipper.Commands;
using Shipper.Script;
using Shipper.TUI;
using System.Diagnostics;
using System.Reflection;
using System.Text;

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
			Console.Write(">> ");
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

	private static Error Run(LineInput input)
	{
		var arguments = input.Arguments;
		ICommand? command = ShipperCore.GetCommand(arguments[0].Content);

		if (command is null)
		{
			Console.WriteLine($"Unknown command: '{arguments[0].Content}'");


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
					Text = new(input.Source, HighlightColor.Announcement),
					Message = new($"Unknown argument No.{i}", HighlightColor.Warning),
					Span = arguments[i].Span,
				};
				highlight.Draw(Console.Out);
			}
		}

		return Error.Ok;
	}

	public static int Main(string[] args)
	{
		ShipperCore.Init();

		if (args.Length == 0)
			return (int)RunInteractive();

		return (int)Run(LineInput.FromArgs(args));
	}

	private static void SetupInteractive()
	{
		RunningInteractive = true;

		Console.Title = "Shipper";
		if (DateTime.Now.Day == 1 && DateTime.Now.Month == 4)
			Console.Title = "Shipper: [INSERT APRIL JOKE]";
	}

	public static bool RunningInteractive { get; private set; } = false;
}
