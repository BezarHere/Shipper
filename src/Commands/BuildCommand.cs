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
			return Error.ExpectedArguments;
		string path = arguments[0].Use(this);
		if (path == "test")
			path = @"F:\Assets\visual studio\Shipper\test.ship";

		if (!File.Exists(path))
		{
			Console.WriteLine($"Project file does not exist: '{path}'");
			return Error.FileDoesNotExist;
		}

		Entry[] entries = ShipScript.Load(File.OpenText(path));

		Dictionary<string, Entry[]> entry_map = new();

		foreach (Entry entry in entries)
		{
			if (entry_map.ContainsKey(entry.Name))
				continue;
			int[] all = entries.IndexOfAll(e => e.Name == entry.Name);
			entry_map[entry.Name] = (from i in all select entries[i]).ToArray();
		}

		Project project = new(entry_map);

		foreach (FilePath fp in project.GetAvailableFiles())
		{
			Console.WriteLine($"found file: '{fp}'");
		}

		return 0;
	}
}
