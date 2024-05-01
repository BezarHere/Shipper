﻿using Shipper.TUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Shipper.Script;

enum TokenType
{
	Unknown,
	WhiteSpace,
	NewLine,

	String,
	LiteralString,

	Assignment,
	Comma,
}

record struct Token(TokenType Type, IndexRange Range, int Index = -1)
{
}

internal static class Tokenizer
{
	public const char CommentPrefix = '#';
	public const char AssignmentChar = '=';
	public static readonly char[] Newline = ['\r', '\n'];

	public static IEnumerable<Token> Run(string source)
	{
		if (string.IsNullOrEmpty(source))
			throw new ArgumentException($"'{nameof(source)}' cannot be null or empty.");

		for (int i = 0; i < source.Length; i++)
		{
			if (Newline.Contains(source[i]))
			{
				int count = source.AsSpan(i).CountContinues(c => Newline.Contains(c));
				yield return new Token(TokenType.NewLine, i..(i + count));
				i += count - 1;
				continue;
			}

			if (char.IsWhiteSpace(source[i]))
			{
				int count = source.AsSpan(i).CountContinues(c => c != '\n' && char.IsWhiteSpace(c));
				yield return new Token(TokenType.WhiteSpace, i..(i + count));
				i += count - 1;
				continue;
			}

			if (source[i] == AssignmentChar)
			{
				yield return new Token(TokenType.Assignment, i..(i + 1));
				continue;
			}

			if (source[i] == ',')
			{
				yield return new Token(TokenType.Comma, i..(i + 1));
				continue;
			}

			if (source[i] == '\'' || source[i] == '"')
			{
				Token tk = ReadString(source, i);
				// something fucked, the string might be unclosed
				if (tk.Range.End > source.Length)
				{
					Highlight highlight = new()
					{
						Message = "Unclosed string",
						Span = i..(i + 1),
						Text = source
					};
					highlight.Draw();
				}

				i = tk.Range.End - 1;
				yield return tk;
				continue;
			}

			if (source[i] == CommentPrefix)
			{
				int newline = source.IndexOf(Newline, i);

				// comment reaches the end of the source
				if (newline == -1)
				{
					break;
				}

				// skip to the new line
				i = newline - 1;
				continue;
			}

			// if non of the above condition is met, then this must be a regular name (string)
			int char_count = source.AsSpan()
				.CountContinues(c => IsIdentifierChar(c, 1), i);

			if (char_count == 0)
			{
				yield return new(TokenType.Unknown, i..(i + 1));
			}

			yield return new(TokenType.String, i..(i + char_count));
			i += char_count - 1;
		}

		yield break;
	}

	private static bool IsIdentifierChar(char c, int offset)
	{
		_ = offset;

		if (char.IsLetterOrDigit(c) || c == '.' || c == '-' || c == '_')
			return true;
		return false;
	}

	private enum StringType
	{
		Normal,
		Literal,
		Multiline,
		LiteralMultiline,
	}

	private static StringType GetStringType(string source, int start)
	{
		int len = source.Length - start;

		// not enough chars
		if (len == 0)
			return StringType.Normal;

		StringType type = source[start] == '"' ? StringType.Normal : StringType.Literal;
		if (len < 6)
			return type;

		if (source[start] == source[start + 1] && source[start + 1] == source[start + 2])
			return type == StringType.Normal ? StringType.Multiline : StringType.LiteralMultiline;
		return type;
	}

	private static Token ReadString(string source, int start)
	{
		StringType type = GetStringType(source, start);
		int type_len = type == StringType.Normal || type == StringType.Literal ? 1 : 3;
		char type_char = source[start];
		int original_start = start;

		start += type_len;

		int end = source.Length - type_len + 1;
		for (int i = start; i < end; i++)
		{
			// escape (in a non-literal string)
			if (source[i] == '\\' && (type == StringType.Normal || type == StringType.Multiline))
			{
				i++;
				continue;
			}

			if (source[i] == type_char)
			{
				// found all the closing quotes
				if (source.AsSpan().CountContinues(c => c == type_char, i) >= type_len)
				{
					// range includes the quotes
					return new(type_char == '"' ? TokenType.String : TokenType.LiteralString, original_start..(i + type_len));
				}
			}

		}

		// the string was not closed!
		return new(type_char == '"' ? TokenType.String : TokenType.LiteralString, original_start..int.MaxValue);
	}


}
