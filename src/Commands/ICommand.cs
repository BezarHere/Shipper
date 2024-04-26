using Shipper;

namespace Shipper.Commands;
interface ICommand
{

	public abstract Error Execute(Argument[] arguments);

	public abstract string Name { get; }
	public abstract string Description { get; }
	public abstract string Help { get; }

}
