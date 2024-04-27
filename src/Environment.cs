using Shipper.Commands;
using Shipper.Script;

namespace Shipper;
static class Environment
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
		List<ICommand> commands_list = new(32)
		{
			new BuildCommand()
		};

		return commands_list.ToArray();
	}

	private static bool initialized = false;
	private static ICommand[] commands = [];
	public static bool Verbose { get; private set; } = false;
	public static FilePath p = new(".");
}
