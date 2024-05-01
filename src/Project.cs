using Shipper.Commands;
using Shipper.Script;

namespace Shipper;

internal class Project
{

	public Project(Dictionary<string, Value> data, FilePath? location)
	{
		Location = location?.Parent ?? FilePath.WorkingDir;
		if (data.TryGetValue("base", out Value base_value))
		{
			Base = new(base_value.String, Location);
		}

		if (data.TryGetValue("header_match", out Value header_match))
		{
			List<Glob> header_match_globs = new();
			foreach (Value entry in header_match.Array)
			{
				header_match_globs.Add(new(entry.String));
			}

			HeaderMatch = header_match_globs.ToArray();
		}


		if (data.TryGetValue("header_unmatch", out Value header_unmatch))
		{
			List<Glob> header_unmatch_globs = new();
			foreach (Value entry in header_unmatch.Array)
			{
				header_unmatch_globs.Add(new(entry.String));

			}

			HeaderUnMatch = header_unmatch_globs.ToArray();
		}

		if (data.TryGetValue("source", out Value source))
		{
			if (source.Type == Script.ValueType.String)
			{
				Source = new(source.String, Location);
			}
		}

		if (data.TryGetValue("target", out Value target))
		{
			if (target.Type == Script.ValueType.String)
			{
				Target = new(target.String, Location);
			}
		}

		if (data.TryGetValue("command", out Value commands_entries))
		{
			List<CommandMacro> commands = new();

			foreach (Value command_entry in commands_entries.Array)
			{
				if (command_entry.Type != Script.ValueType.Array)
				{
					// error?
					continue;
				}
				var args = from v in command_entry.Array[1..] select v.String;
				commands.Add(new(command_entry.Array[0].String, LineInput.FromArgs(args.ToArray())));
			}

			Commands = commands.ToArray();
		}

	}

	public static Project FromFile(FilePath filePath)
	{
		var entries = ShipScript.Load(File.ReadAllText(filePath)).ToArray();
		return new(new Dictionary<string, Value>(entries), filePath);
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
