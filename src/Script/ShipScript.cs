using System.Text;

using Shipper.Exceptions;
namespace Shipper.Script;

static class ShipScript
{

	public static Dictionary<string, Value> Load(Stream reader)
	{
		long len = reader.Length - reader.Position;
		Span<byte> bytes = new byte[len];
		reader.Read(bytes);
		string text = Encoding.UTF8.GetString(bytes);
		return Load(text);
	}

	public static Dictionary<string, Value> Load(string source)
	{
		Dictionary<string, Value> dict = [];

		IEnumerable<Token> tokens = Tokenizer.Run(source);

		ExpectingType expecting = ExpectingType.Key;
		ValueKey key = default;
		IDictionary<string, Value> working_dict = dict;

		int line_no = 0;
		int line_pos = 0;

		ShipParsingException CreateException(string message, in Token tk)
		{
			return new(message, line_no, tk.Range.Start - line_pos);
		}

		foreach (Token tk in tokens)
		{
			if (tk.Type == TokenType.WhiteSpace)
				continue;
			if (tk.Type == TokenType.NewLine)
			{
				line_no += tk.Range.Length;
				line_pos = tk.Range.End;
			}

			if (expecting == ExpectingType.Key)
			{
				if (tk.Type == TokenType.NewLine)
					continue;

				if (!tk.IsString)
				{
					// expecting a key but found a unexpected token
					throw CreateException($"Unexpected '{source.Substring(tk.Range)}', expected a key name", tk);

				}

				expecting = ExpectingType.Assignment;
				key = new ValueKey(source.Substring(tk.Range));
				working_dict = key.GetParent(dict) ?? throw new AccessViolationException($"'{key.Raw}' does not exist");
				if (!working_dict.TryGetValue(key.Name, out Value val))
				{
					working_dict.Add(key.Name, val = new Value());
				}

				// redefinition of an array will turn it into an array list (deep list)
				if (val.Type == ValueType.List)
				{
					List<string> contained = val.List;
					val = new Value([contained, new List<string>()]);
					working_dict[key.Name] = val;
				}
				else if (val.Type == ValueType.Table)
				{
					// there is nothing legal the user can do directly to a table (for the time being)
					throw CreateException($"'{key.Raw}' is a table, which can't be access directly", tk);
				}

				continue;
			}

			if (expecting == ExpectingType.Assignment)
			{
				if (tk.Type != TokenType.Assignment)
				{
					// expecting an operator but found a unexpected token
					throw CreateException($"Unexpected '{source.Substring(tk.Range)}', expected an assignment", tk);
				}

				expecting = ExpectingType.Value;
				continue;
			}

			if (expecting == ExpectingType.Value)
			{
				// will mark any trailing comma as a parsing error
				if (tk.Type == TokenType.NewLine)
				{
					expecting = ExpectingType.Key;
					continue;
				}

				if (!tk.IsString)
				{
					// expecting a value but found a unexpected token
					throw CreateException($"Unexpected '{source.Substring(tk.Range)}', expected a value", tk);
				}

				expecting = ExpectingType.CommaOrTermination;

				if (string.IsNullOrEmpty(key.Name))
				{
					throw new NullReferenceException($"'{nameof(key.Name)}' can't be null or empty");
				}

				Value val = working_dict[key.Name];
				// if true, it needs to be updated
				if (AddValue(ref val, source.Substring(tk.Range)))
				{
					working_dict[key.Name] = val;
				}
				continue;
			}

			if (expecting == ExpectingType.CommaOrTermination)
			{
				if (tk.Type == TokenType.NewLine)
				{
					expecting = ExpectingType.Key;
					continue;
				}

				if (tk.Type != TokenType.Comma)
				{
					throw CreateException($"Unexpected '{source.Substring(tk.Range)}', expected a comma to separate values", tk);
				}

				expecting = ExpectingType.Value;
				continue;
			}
		}

		return dict;
	}

	public static void Dump(TextWriter writer, KeyValuePair<string, Value> values)
	{
	}


	/// <summary>
	/// 
	/// </summary>
	/// <param name="parent"></param>
	/// <param name="new_value"></param>
	/// <returns>weather the value should be updated</returns>
	/// <exception cref="InvalidOperationException">if trying to add to a table</exception>
	private static bool AddValue(ref Value parent, string new_value)
	{
		switch (parent.Type)
		{
			// set it to the new value as a string
			case ValueType.None:
				parent = new Value(new_value);
				return true;
			// convert the string to an array of 
			case ValueType.String:
			{
				string contained = parent.String;
				parent = new Value([contained, new_value]);
				return true;
			}
			case ValueType.List:
			{
				parent.List.Add(new_value);
				return false;
			}
			case ValueType.DeepList:
			{
				parent.DeepList[^1].Add(new_value);
				return false;
			}
			case ValueType.Table:
				throw new InvalidOperationException($"{nameof(parent)} is a table, not an array");
			default:
				throw new InvalidOperationException($"'{parent.Type}' isn't a valid value type");
		}
	}

	private readonly struct ValueKey
	{
		public ValueKey(string raw)
		{
			Raw = raw;
			Segments = (from i in ParseSegments(raw) where i.Valid select i).ToArray();
			Name = GetSegment(Segments.Length - 1);
		}

		public readonly string GetSegment(int index)
		{
			return Raw.Substring(Segments[index]);
		}

		public readonly ReadOnlySpan<char> GetSegmentAsSpan(int index)
		{
			return Raw.AsSpan(Segments[index]);
		}

		public readonly IDictionary<string, Value>? GetParent(IDictionary<string, Value> dict)
		{
			Value val = default;
			for (int i = 0; i < Segments.Length - 1; i++)
			{
				if (!dict.TryGetValue(GetSegment(i), out val))
				{
					// value doesn't exist? make it
					val = new Value(ValueType.Table);
					dict.Add(GetSegment(i), val);
				}

				// the value isn't a table, but we expect a table
				if (val.Type != ValueType.Table)
					return null;

				dict = val.Table;
			}

			return dict;
		}

		public readonly string Raw = string.Empty;
		public readonly IndexRange[] Segments = [];
		public readonly string Name = string.Empty;

		private static IEnumerable<IndexRange> ParseSegments(string raw)
		{
			int index = 0;
			while (index != -1)
			{
				int new_index = raw.IndexOf('.', index);

				if (new_index == -1)
				{
					yield return index..raw.Length;
					break;
				}

				yield return index..new_index;
				index = new_index + 1;
			}
		}

	}

	private enum ExpectingType
	{
		Key,
		Assignment,
		Value,
		CommaOrTermination,
	}

}
