using Shipper.Commands;
using Shipper.Script;

namespace Shipper;

internal class Project
{
	private record struct HeaderTransformer(in Glob Matcher, string Target)
	{
	}

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
			
			if (header_match.Type == Script.ValueType.DeepList)
			{
				header_match = Value.ToListValue(header_match);
			}

			foreach (string entry in header_match.List)
			{
				header_match_globs.Add(new(entry));
			}

			HeaderMatch = header_match_globs.ToArray();
		}


		if (data.TryGetValue("header_unmatch", out Value header_unmatch))
		{
			List<Glob> header_unmatch_globs = new();
			if (header_unmatch.Type == Script.ValueType.DeepList)
			{
				header_unmatch = Value.ToListValue(header_unmatch);
			}

			foreach (string entry in header_unmatch.List)
			{
				header_unmatch_globs.Add(new(entry));

			}

			HeaderBlacklist = header_unmatch_globs.ToArray();
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
			List<CommandMacro> commands = [];

			foreach (List<string> command_entry in commands_entries.DeepList)
			{
				commands.Add(new(command_entry[0], LineInput.FromArgs(command_entry.ToArray()[1..])));
			}

			Commands = commands.ToArray();
		}

		PostProcessPaths();
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

	public IEnumerable<FilePath> GetAvailableFiles()
	{
		Stack<FilePath> directories = new(1024);
		directories.Push(Base);

		while (directories.Count > 0)
		{
			FilePath current = directories.Pop();
			if (!current.IsDirectory)
				continue;

			foreach (FilePath fp in current.GetDirectories())
				directories.Push(fp);

			foreach (FilePath fp in current.GetFiles())
				yield return fp;
		}
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

	private void PostProcessPaths()
	{
		Dictionary<string, string> macros = new()
		{
			["__base__"] = Base,
			["__location__"] = Location,
			["__source__"] = Source,
			["__target__"] = Target,
		};

		string Subtitute(string original)
		{
			foreach (var (k, v) in macros)
			{
				original = original.Replace(k, v);
			}
			return original;
		}

		Base = new FilePath(Subtitute(Base));
		Location = new FilePath(Subtitute(Location));
		Source = new FilePath(Subtitute(Source));
		Target = new FilePath(Subtitute(Target));

		for (int i = 0; i < Commands.Length; i++)
		{
			var original_args = Commands[i].Input.Arguments;
			var args_count = original_args.Length;
			string[] arguments = new string[args_count];
			for (int j = 0; j < args_count; j++)
			{
				arguments[j] = Subtitute(original_args[j].Content);
			}
			Commands[i] = new(Commands[i].Name, LineInput.FromArgs(arguments));
		}

	}

	private bool IsHeaderPathIncluded(in FilePath path)
	{
		for (int i = 0; i < HeaderMatch.Length; i++)
		{

			if (HeaderMatch[i].Test(path, true))
			{
				bool excluded = false;
				for (int j = 0; j < HeaderBlacklist.Length; j++)
				{
					// the path is unmatched, break
					if (HeaderBlacklist[j].Test(path, true))
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

	public FilePath Base { get; private set; }
	public FilePath Location { get; private set; }

	public FilePath Source { get; private set; }
	public FilePath Target { get; private set; }

	public Glob[] HeaderMatch { get; private set; } = [];


	public Glob[] HeaderBlacklist { get; private set; } = [];

	public CommandMacro[] Commands { get; private set; } = [];

}
