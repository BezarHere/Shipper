using Shipper.Commands;
using Shipper.Script;
using System.IO;

namespace Shipper;

internal class Project
{

	public Project(Dictionary<string, Entry[]> data, FilePath? location)
	{
		Location = location?.Parent ?? FilePath.WorkingDir;
		if (data.TryGetValue("base", out Entry[]? base_value) && base_value[0].Values.Length > 0)
		{
			Base = new(base_value[0].Values[0], Location);
		}

		if (data.TryGetValue("header_match", out Entry[]? header_match))
		{
			List<Glob> header_match_globs = new();
			foreach (Entry entry in header_match)
			{
				foreach (string value in entry.Values)
				{
					header_match_globs.Add(new(value));
				}
			}

			HeaderMatch = header_match_globs.ToArray();
		}


		if (data.TryGetValue("header_unmatch", out Entry[]? header_unmatch))
		{
			List<Glob> header_unmatch_globs = new();
			foreach (Entry entry in header_unmatch)
			{
				foreach (string value in entry.Values)
				{
					header_unmatch_globs.Add(new(value));
				}
			}

			HeaderUnMatch = header_unmatch_globs.ToArray();
		}

		if (data.TryGetValue("source", out Entry[]? source))
		{
			if (source[0].Values.Length > 0)
			{
				Source = new(source[0].Values[0], Location);
			}
		}

		if (data.TryGetValue("target", out Entry[]? target))
		{
			if (target[0].Values.Length > 0)
			{
				Target = new(target[0].Values[0], Location);
			}
		}

		if (data.TryGetValue("command", out Entry[]? commands_entries))
		{
			List<CommandMacro> commands = new();

			foreach (Entry command_entry in commands_entries)
			{
				if (command_entry.Values.Length < 1)
				{
					// error?
					continue;
				}
				commands.Add(new(command_entry.Values[0], LineInput.FromArgs(command_entry.Values[1..])));
			}

			Commands = commands.ToArray();
		}

	}

	public static Project FromFile(FilePath filePath)
	{
		Entry[] entries = ShipScript.Load(File.OpenText(filePath)).ToArray();

		Dictionary<string, Entry[]> entry_map = new();

		foreach (Entry entry in entries)
		{
			if (entry_map.ContainsKey(entry.Name))
				continue;
			entry_map[entry.Name] = (from e in entries where e.Name == entry.Name select e).ToArray();
		}

		return new(entry_map, filePath);
	}

	public Error Start()
	{
		return Error.Ok;
	}

	public IEnumerable<FilePath> GetHeaderFiles()
	{
		return from fp in GetAvailableFiles() where IsHeaderPathIncluded(fp) select fp;
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

	private Error[] RunCommands()
	{
		// TODO: ERORR REPORTING
		int counter = 0;
		Error[] results = new Error[Commands.Length];
		foreach (CommandMacro macro in Commands)
		{
			ICommand? command = ShipperCore.GetCommand(macro.Name);
			if (command is null)
			{
				results[counter++] = Error.UnknownCommand;
				continue;
			}

			results[counter++] = command.Execute(macro.Input.Arguments, CommandCallContext.ProjectCommand);
		}
		return results;
	}

	private bool IsHeaderPathIncluded(in FilePath path)
	{
		for (int i = 0; i < HeaderMatch.Length; i++)
		{

			if (HeaderMatch[i].Test(path, true))
			{
				bool excluded = false;
				for (int j = 0; j < HeaderUnMatch.Length; j++)
				{
					// the path is unmatched, break
					if (HeaderUnMatch[j].Test(path, true))
					{
						excluded = true;
						break;
					}
				}

				if (excluded)
					continue;

				return true;
			}
		}

		return false;
	}

	public readonly FilePath Base;
	public readonly FilePath Location;

	public readonly FilePath Source;
	public readonly FilePath Target;

	public readonly Glob[] HeaderMatch = [];
	public readonly Glob[] HeaderUnMatch = [];

	public CommandMacro[] Commands { get; private set; } = [];

}
