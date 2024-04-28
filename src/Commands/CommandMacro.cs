namespace Shipper.Commands;
internal struct CommandMacro(string name, LineInput? input = null)
{
	public string Name = name;
	public LineInput Input = input ?? new();
}
