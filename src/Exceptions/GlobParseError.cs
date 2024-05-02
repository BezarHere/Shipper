namespace Shipper.Exceptions;

internal class GlobParseError(string message, IndexRange range, string line) : Exception(message)
{
	public readonly IndexRange Range = range;
	public readonly string Line = line;
}
