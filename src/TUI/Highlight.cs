using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shipper.TUI;
enum HighlightColor
{
	Debug,
	Verbose = Debug,
	Normal,
	Info = Normal,
	Announcement,
	Warning,
	Error,
	Critical,
}

internal struct Highlight : ITUI
{
	public Highlight()
	{
	}

	public void Draw(TextWriter writer)
	{
		if (Text.String.Length > 0)
			Write(writer, Text);

		if (Span.Valid)
			Write(writer, new(HighlightArrows(), Message.Color));

		if (Message.String.Length > 0)
		{
			if (Span.Start > 3)
			{
				writer.Write(new string('.', Span.Start - 2));
				writer.Write(' ');
			}

			Write(writer, Message);
		}
	}

	public void Draw() => Draw(Console.Out);

	private readonly void Write(TextWriter writer, in ColorString str)
	{
		writer.Write(new string(' ', Offset));

		if (writer == Console.Out)
		{
			ConsoleColor old_color = Console.ForegroundColor;
			Console.ForegroundColor = str.Color.ConsoleColor();
			writer.WriteLine(str.String);
			Console.ForegroundColor = old_color;
			return;
		}

		// not the console output, no magic colors :(
		writer.WriteLine(str.String);
	}

	private string HighlightArrows()
	{
		Span<char> span = stackalloc char[Text.String.Length];

		for (int i = 0; i < span.Length; i++)
		{
			if (Span.Contains(i))
				span[i] = '^';
			else
				span[i] = ' ';
		}

		return new(span);
	}
	public struct ColorString(string str, HighlightColor color = HighlightColor.Normal)
	{
		public static implicit operator ColorString(string text)
		{
			return new(text);
		}
		public static implicit operator string(ColorString text)
		{
			return text.String;
		}

		public string String = str;
		public HighlightColor Color = color;
	}

	public ushort Offset = 0;
	public ColorString Text = "sample";
	public ColorString Message = "message";
	public IndexRange Span = new(0, 4);

}
