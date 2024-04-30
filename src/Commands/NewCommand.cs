namespace Shipper.Commands;

readonly struct ProjectNewParameters
{
	public readonly FilePath Path;
	public readonly LineInput Input;
}

interface IProjectNewTemplate
{
	public string Name { get; }
	public string Description { get; }

	public string Generate(in ProjectNewParameters parameters);

}

internal class NewCommand : ICommand
{
	public string Name => "new";

	public string Description => "creates a new template project file";

	public string Help => $"{Name} [<template>] [<path=\"WorkingDir/WorkingDirName.ship\">]";

	public Error Execute(Argument[] arguments, CommandCallContext context = CommandCallContext.InteractiveTerminal)
	{
		throw new NotImplementedException();
	}
}
