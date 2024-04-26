
namespace Shipper;

internal class Program
{
	public static int RunInteractive()
	{
		Console.WriteLine("Starting the interactive terminal:");

		RunningInteractive = true;
		int result = 0;

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
			int operation_result = Run(arguments);

			// non-zero result indicate an error
			if (operation_result != 0)
			{
				result = operation_result;
				break;
			}
		}
		return result;
	}

	private static int Run(Argument[] arguments)
	{
		for (int i = 0; i < arguments.Length; i++)
		{
			Console.WriteLine($"{arguments[i]}");
		}
		return 0;
	}

	public static int Main(string[] args)
	{
		if (args.Length == 0)
			return RunInteractive();

		Argument[] arguments = new Argument[args.Length];
		for (int i = 0; i < args.Length; i++)
		{
			arguments[i] = new(args[i]);
		}

		return Run(arguments);
	}

	public static Environment Environment { get; private set; } = new();
	public static bool RunningInteractive { get; private set; } = false;
}
