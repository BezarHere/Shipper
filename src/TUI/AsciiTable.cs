
namespace Shipper.TUI;

internal class AsciiTable : ITUI
{

	public void Draw(TextWriter writer)
	{
		throw new NotImplementedException();
	}

	public void Draw() => Draw(Console.Out);


	// rows<columns>
	public List<string[]> Rows = [];

}
