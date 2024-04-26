namespace Shipper;
readonly internal struct LineInput(string source, Argument[] arguments)
{
	public readonly string Source = source;
	public readonly Argument[] Arguments = arguments;
}
