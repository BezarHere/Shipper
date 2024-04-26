namespace Shipper.Commands;

class BuildCommand : ICommand
{
	public string Name => "build";

	public string Description => "builds the given project";

	public string Help => "stuff";

	public Error Execute(Argument[] arguments)
	{
		if (arguments.Length == 0)
			return Error.ExpectedArguments;
		string path = arguments[0].Use(this);


		return 0;
	}
}
