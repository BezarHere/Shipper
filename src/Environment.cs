using Shipper.Commands;

namespace Shipper;
class Environment
{
	public Environment()
	{
		Commands =
		[
			new BuildCommand(),
		];
	}

	public ICommand? GetCommand(string name)
	{
		foreach (ICommand command in Commands)
		{
			if (command.Name == name)
				return command;
		}
		return null;
	}

	public ICommand[] Commands { get; init; }
}
