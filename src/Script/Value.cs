using System;

namespace Shipper.Script;

enum ValueType
{
	None,
	String,
	List,
	ArrayList,
	Table,
}

internal struct Value : IEquatable<Value>
{
	private object _data = "";
	public readonly ValueType Type { get; init; }

	/// <summary>
	/// creates a new value of type <paramref name="type"/> and sets it's data to the default value
	/// </summary>
	/// <param name="type">the value type</param>
	public Value(ValueType type = ValueType.None)
	{
		Type = type;

		switch (Type)
		{
			case ValueType.None:
			{
				_data = 0;
				break;
			}
			case ValueType.String:
			{
				_data = string.Empty;
				break;
			}
			case ValueType.List:
			{
				_data = new List<string>();
				break;
			}
			case ValueType.ArrayList:
			{
				_data = new List<List<string>>();
				break;
			}
			case ValueType.Table:
			{
				_data = new Dictionary<string, Value>();
				break;
			}
		}

	}

	public Value(string value)
	{
		Type = ValueType.String;
		_data = value;
	}

	public Value(List<string> list)
	{
		Type = ValueType.List;
		_data = list;
	}

	public Value(List<List<string>> array_list)
	{
		Type = ValueType.ArrayList;
		_data = array_list;
	}

	public Value(Dictionary<string, Value> table)
	{
		Type = ValueType.Table;
		_data = table;
	}

	/// <summary>
	/// converts an array list (list of lists of strings) to a list (list of strings), flattening all the nested lists.
	/// </summary>
	/// <returns>a list value if the value is an array list, otherwise returns an empty value</returns>
	public static Value ToListValue(in Value value)
	{
		if (value.Type != ValueType.ArrayList)
			return default;

		List<string> list = [];
		foreach (List<string> ls in value.ArrayList)
		{
			list.AddRange(ls);
		}
		return new Value(list);
	}

	public string String
	{
		readonly get
		{
			if (Type != ValueType.String)
				throw new InvalidOperationException("Value isn't a string type");
			return (string)_data;
		}
		set
		{
			if (Type != ValueType.String)
				throw new InvalidOperationException("Value isn't a string type");
			_data = value;
		}
	}

	public List<string> List
	{
		readonly get
		{
			if (Type != ValueType.List)
				throw new InvalidOperationException("Value isn't a list type");
			return (List<string>)_data;
		}
		set
		{
			if (Type != ValueType.List)
				throw new InvalidOperationException("Value isn't a list type");
			_data = value;
		}
	}

	public List<List<string>> ArrayList
	{
		readonly get
		{
			if (Type != ValueType.ArrayList)
				throw new InvalidOperationException("Value isn't a list array type");
			return (List<List<string>>)_data;
		}
		set
		{
			if (Type != ValueType.ArrayList)
				throw new InvalidOperationException("Value isn't a list array type");
			_data = value;
		}
	}

	public Dictionary<string, Value> Table
	{
		readonly get
		{
			if (Type != ValueType.Table)
				throw new InvalidOperationException("Value isn't a table type");
			return (Dictionary<string, Value>)_data;
		}
		set
		{
			if (Type != ValueType.Table)
				throw new InvalidOperationException("Value isn't a table type");
			_data = value;
		}
	}


	public readonly bool Equals(Value other)
	{
		return Type == other.Type && _data == other._data;
	}

	public override readonly bool Equals(object? obj)
	{
		return obj is Value value && Equals(value);
	}

	public override readonly int GetHashCode()
	{
		return _data.GetHashCode();
	}
}
