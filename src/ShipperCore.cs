using Shipper.Commands;
using Shipper.Script;

namespace Shipper;
static class ShipperCore
{
	public static ICommand? GetCommand(string name)
	{
		foreach (ICommand command in commands)
		{
			if (command.Name == name)
				return command;
		}
		return null;
	}

	internal static void Init()
	{
		if (initialized) return;
		initialized = true;
		commands = GenerateCommands();
	}

	private static ICommand[] GenerateCommands()
	{
		ICommand[] commands_array =
		[
			new BuildCommand(),

			new StatsCommand(),
			new ProxyCommand<StatsCommand>("stat"),

			new CopyCommand(),
			new ProxyCommand<CopyCommand>("cp"),

			new MoveCommand(),
			new ProxyCommand<MoveCommand>("rename"),

			new DeleteCommand(),
			new ProxyCommand<DeleteCommand>("del"),
			new ProxyCommand<DeleteCommand>("rm"),
		];

		return commands_array;
	}

	private static bool initialized = false;
	private static ICommand[] commands = [];
	public static bool Verbose { get; private set; } = false;
	public static FilePath p = new(".");
}
