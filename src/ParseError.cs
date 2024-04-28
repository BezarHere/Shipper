namespace Shipper;

internal class ParseError(string? message = null, IndexRange? span = null, string? source = null) : InvalidOperationException(message)
{
	public readonly IndexRange Span = span ?? new();
	public readonly string? SourceCode = source;
}
