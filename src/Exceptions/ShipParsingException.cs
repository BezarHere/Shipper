namespace Shipper.Exceptions;

internal class ShipParsingException(string message, long line_no = -1, long column = -1) : Exception(message)
{
	public readonly long LineNumber = line_no;
	public readonly long Column = column;
}
