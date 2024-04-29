namespace Shipper.Commands;

internal class CreateCommand : ICommand
{
	public string Name => "create";

	public string Description => "creates a new template project file";

	public string Help => "create [<template>] [<path=\"WorkingDir/WorkingDirName.ship\">]";

	public Error Execute(Argument[] arguments)
	{
		throw new NotImplementedException();
	}
}
