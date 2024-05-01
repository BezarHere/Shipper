using System.Text;

namespace Shipper.Script;

static class ShipScript
{

	public static IEnumerable<KeyValuePair<string, Value>> Load(Stream reader)
	{
		long len = reader.Length - reader.Position;
		Span<byte> bytes = new byte[len];
		reader.Read(bytes);
		string text = UTF8Encoding.UTF8.GetString(bytes);
		return Load(text);
	}

	public static IEnumerable<KeyValuePair<string, Value>> Load(string source)
	{
		foreach (var tk in Tokenizer.Run(source))
		{
			Console.WriteLine($"token: {tk.Type}, {tk.Range}, '{source[tk.Range.Start..tk.Range.End]}'");
		}

		Token[] tokens = Tokenizer.Run(source).ToArray();

		yield break;
	}

	public static void Dump(TextWriter writer, KeyValuePair<string, Value> values)
	{
	}



}
