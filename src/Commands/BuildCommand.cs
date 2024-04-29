using Shipper.Script;

namespace Shipper.Commands;

class BuildCommand : ICommand
{
	public string Name => "build";

	public string Description => "builds the given project";

	public string Help => "stuff";

	public Error Execute(Argument[] arguments)
	{
		if (arguments.Length == 0)
			return Error.ExpectedArgument;
		FilePath path = new(arguments[0].Use(this));

		if (string.IsNullOrEmpty(path.Extension))
		{
			path = new($"{path}.ship");
		}

		if (!path.Exists)
		{
			Console.WriteLine($"Project file does not exist: '{path}'");
			return Error.FileDoesNotExist;
		}

		Project project = Project.FromFile(path);

		foreach (FilePath fp in project.GetAvailableFiles())
		{
			Console.WriteLine($"found file: '{fp}'");
		}

		foreach (FilePath fp in project.GetHeaderFiles())
		{
			Console.WriteLine($"included file: '{fp}'");
		}

		return 0;
	}
}
