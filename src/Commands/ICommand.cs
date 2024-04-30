using Shipper;

namespace Shipper.Commands;

enum CommandCallContext
{
	ProgramStartup,
	InteractiveTerminal,
	ProjectCommand
}

interface ICommand
{
	public abstract Error Execute(Argument[] arguments, CommandCallContext context = CommandCallContext.InteractiveTerminal);

	public abstract string Name { get; }
	public abstract string Description { get; }
	public abstract string Help { get; }

}
