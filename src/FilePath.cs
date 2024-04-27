using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shipper;

internal readonly struct FilePath
{

	public FilePath(string path)
	{
		ArgumentNullException.ThrowIfNull(path);
		this.path = Path.GetFullPath(path);
	}

	public override string ToString()
	{
		return path;
	}

	public readonly bool Exists { get => Path.Exists(path); }
	public readonly bool IsDirectory { get => Directory.Exists(path); }
	public readonly bool IsFile { get => File.Exists(path); }

	private readonly string path;
}
