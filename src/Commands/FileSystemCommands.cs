namespace Shipper.Commands;

/*
* TODO: make those commands use an underprivileged thread for safety
*/

internal class CopyCommand : ICommand
{
	public string Name => "copy";

	public string Description => "copies a file to another location";

	public string Help => "copy <source> <destination> ['-O' or '-overwrite' to force overwrite]";

	public Error Execute(Argument[] arguments, CommandCallContext context = CommandCallContext.InteractiveTerminal)
	{
		if (arguments.Length < 2)
		{
			return Error.ExpectedArgument;
		}

		bool overwrite = false;
		if (arguments.Length >= 3)
		{
			if (arguments[2].Content == "-O" || arguments[2].Content == "-overwrite")
			{
				overwrite = true;
				_ = arguments[2].Use(this);
			}
		}

		try
		{
			File.Copy(arguments[0].Use(this), arguments[1].Use(this), overwrite);
		}
		catch (UnauthorizedAccessException)
		{
			return Error.UnauthorizedAccess;
		}
		catch (PathTooLongException)
		{
			return Error.PathTooLong;
		}
		catch (DirectoryNotFoundException)
		{
			return Error.DirectoryDoesNotExist;
		}
		catch (FileNotFoundException)
		{
			return Error.FileDoesNotExist;
		}
		catch (NotSupportedException)
		{
			return Error.UnsupportedOperation;
		}

		return Error.Ok;
	}
}

internal class MoveCommand : ICommand
{
	public string Name => "move";

	public string Description => "moves a file to another location";

	public string Help => "move <source> <destination> ['-O' or '-overwrite' to force overwrite]";

	public Error Execute(Argument[] arguments, CommandCallContext context = CommandCallContext.InteractiveTerminal)
	{
		if (arguments.Length < 2)
		{
			return Error.ExpectedArgument;
		}

		bool overwrite = false;
		if (arguments.Length >= 3)
		{
			if (arguments[2].Content == "-O" || arguments[2].Content == "-overwrite")
			{
				overwrite = true;
				_ = arguments[2].Use(this);
			}
		}

		try
		{
			File.Move(arguments[0].Use(this), arguments[1].Use(this), overwrite);
		}
		catch (UnauthorizedAccessException)
		{
			return Error.UnauthorizedAccess;
		}
		catch (PathTooLongException)
		{
			return Error.PathTooLong;
		}
		catch (DirectoryNotFoundException)
		{
			return Error.DirectoryDoesNotExist;
		}
		catch (FileNotFoundException)
		{
			return Error.FileDoesNotExist;
		}
		catch (NotSupportedException)
		{
			return Error.UnsupportedOperation;
		}

		return Error.Ok;
	}
}

internal class DeleteCommand : ICommand
{

	public string Name => "delete";

	public string Description => "deletes a file";

	public string Help => "delete <target>";

	public Error Execute(Argument[] arguments, CommandCallContext context = CommandCallContext.InteractiveTerminal)
	{
		if (arguments.Length < 1)
		{
			return Error.ExpectedArgument;
		}

		try
		{
			File.Delete(arguments[0].Use(this));
		}
		catch (UnauthorizedAccessException)
		{
			return Error.UnauthorizedAccess;
		}
		catch (PathTooLongException)
		{
			return Error.PathTooLong;
		}
		catch (DirectoryNotFoundException)
		{
			return Error.DirectoryDoesNotExist;
		}
		catch (NotSupportedException)
		{
			return Error.UnsupportedOperation;
		}

		return Error.Ok;
	}
}

