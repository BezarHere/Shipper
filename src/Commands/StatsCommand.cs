using System.Diagnostics;

namespace Shipper.Commands;
internal class StatsCommand : ICommand
{
	public string Name => "stats";

	public string Description => "Prints out the current process stats";

	public string Help => "TODO";

	public Error Execute(Argument[] arguments, CommandCallContext context = CommandCallContext.InteractiveTerminal)
	{
		Process process = Process.GetCurrentProcess();
		PrintMemoryStats(process);
		return Error.Ok;
	}

	public static void PrintMemoryStats(Process process)
	{
		// TODO: add a table TUI
		Console.WriteLine($"ID: {process.Id}");
		Console.WriteLine($"Name: {process.ProcessName}");
		Console.WriteLine($"Handle count: {process.HandleCount}");
		Console.WriteLine($"Session ID: {process.SessionId}");
		Console.WriteLine($"Start time: {process.StartTime}");
		Console.WriteLine($"Threads count: {process.Threads.Count}");
		Console.WriteLine($"Parent Dir: {FilePath.ParentDir}");
		Console.WriteLine($"Working Dir: {FilePath.WorkingDir}");
		Console.WriteLine($"Memory:");
		Console.WriteLine($"  paged memory: {process.PagedMemorySize64.AsMegabyte()}mb [{process.PeakPagedMemorySize64.AsMegabyte()}mb]");
		Console.WriteLine($"  working memory: {process.WorkingSet64.AsMegabyte()}mb [{process.PeakWorkingSet64.AsMegabyte()}mb]");
		Console.WriteLine($"  private memory: {process.PrivateMemorySize64.AsMegabyte()}mb");
	}
}
