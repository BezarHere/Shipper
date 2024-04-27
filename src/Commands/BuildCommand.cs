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

		Entry[] entries = ShipScript.Load(File.OpenText(path));

		Dictionary<string, Entry[]> entry_map = new();

		foreach (Entry entry in entries)
		{
			if (entry_map.ContainsKey(entry.Name))
				continue;
			int[] all = entries.IndexOfAll(e => e.Name == entry.Name);
			entry_map[entry.Name] = (from i in all select entries[i]).ToArray();
		}

		return 0;
	}
}
