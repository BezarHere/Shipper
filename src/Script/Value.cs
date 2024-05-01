namespace Shipper.Script;

enum ValueType
{
	String,
	Array,
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
	public Value(ValueType type = ValueType.String)
	{
		Type = type;
		if (type == ValueType.String)
		{
			_data = string.Empty;
		}
		else if (type == ValueType.Array)
		{
			_data = System.Array.Empty<Value>();
		}
		else
		{
			_data = new Dictionary<string, Value>();
		}

	}

	public Value(string value)
	{
		Type = ValueType.String;
		_data = value;
	}

	public Value(Value[] array)
	{
		Type = ValueType.Array;
		_data = array;
	}

	public Value(Dictionary<string, Value> table)
	{
		Type = ValueType.Table;
		_data = table;
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

	public Value[] Array
	{
		readonly get
		{
			if (Type != ValueType.Array)
				throw new InvalidOperationException("Value isn't an array type");
			return (Value[])_data;
		}
		set
		{
			if (Type != ValueType.Array)
				throw new InvalidOperationException("Value isn't an array type");
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
