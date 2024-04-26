using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shipper.TUI;
internal class Highlight : ITUI
{
	public void Draw(TextWriter writer)
	{
		if (Text.Length > 0)
			Write(writer, Text);
		if (Range.Valid)
			Write(writer, HighlightArrows());
		if (Message.Length > 0)
			Write(writer, Message);
	}

	private void Write(TextWriter writer, string str)
	{
		writer.Write(new string(' ', Offset));
		writer.WriteLine(str);
	}

	private string HighlightArrows()
	{
		Span<char> span = stackalloc char[Text.Length];

		for (int i = 0; i < span.Length; i++)
		{
			if (Range.Contains(i))
				span[i] = '^';
			else
				span[i] = ' ';
		}

		return new(span);
	}

	public ushort Offset = 0;
	public string Text = "sample";
	public string Message = "message";
	public SpanRange Range = new(0, 4);
}
