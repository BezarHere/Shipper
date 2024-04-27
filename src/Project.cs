using Shipper.Script;

namespace Shipper;

internal class Project
{
	public Project(Dictionary<string, Entry[]> data)
	{
		if (data.ContainsKey("base") && data["base"].Length > 0)
			Base = new(data["base"][0].Values[0]);
	}

	public FilePath[] GetAvailableFiles()
	{
		List<FilePath> results = new();
		Stack<FilePath> directories = new();
		directories.Push(Base);

		while (directories.Count > 0)
		{
			FilePath current = directories.Pop();
			if (!current.IsDirectory)
				continue;

			foreach (FilePath fp in current.GetDirectories())
				directories.Push(fp);

			foreach (FilePath fp in current.GetFiles())
				results.Add(fp);
		}
		return results.ToArray();
	}

	public FilePath Base { get; private set; }
}
