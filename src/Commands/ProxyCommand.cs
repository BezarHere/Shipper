namespace Shipper.Commands;

internal class ProxyCommand<T>(string name) : ICommand where T : ICommand, new()
{

	public string Name => name;

	public string Description => _proxy.Description;

	public string Help => _proxy.Help;

	public Error Execute(Argument[] arguments, CommandCallContext context = CommandCallContext.InteractiveTerminal)
	=> _proxy.Execute(arguments, context);


	private readonly T _proxy = new();
}
