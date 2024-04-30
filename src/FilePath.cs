using System.Reflection;
using System.Text;

namespace Shipper;

internal readonly struct FilePath
{

	public FilePath(string path) : this(path, FilePath.WorkingDir)
	{
	}

	public FilePath(string path, in FilePath base_path)
	{
		ArgumentNullException.ThrowIfNull(path);
		Content = Resolve(path, base_path.Content);
	}

	public override string ToString()
	{
		return Content;
	}

	public static implicit operator string(FilePath path)
	{
		return path.Content;
	}

	public static explicit operator FileInfo(FilePath filePath)
	{
		return new(filePath.Content);
	}

	public static explicit operator DirectoryInfo(FilePath filePath)
	{
		return new(filePath.Content);
	}

	public readonly IEnumerable<FilePath> GetFiles()
	{
		if (!IsDirectory)
			return Enumerable.Empty<FilePath>();
		return from dir in Directory.EnumerateFiles(this.Content) select new FilePath(dir);
	}

	public readonly IEnumerable<FilePath> GetDirectories()
	{
		if (!IsDirectory)
			return Enumerable.Empty<FilePath>();
		return from dir in Directory.EnumerateDirectories(this.Content) select new FilePath(dir);
	}

	/// <summary>
	/// hashes key attributes about the target file; theoretically giving a different hash each time the file changes.
	/// </summary>
	/// <returns>a hash code generated for the target file</returns>
	public readonly int GetFileHashCode()
	{
		if (!IsFile)
			return ~0;
		FileInfo fileInfo = new(this.Content);
		return fileInfo.Name.GetHashCode() ^ fileInfo.Length.InterlaceHalfBits()
			^ fileInfo.LastWriteTime.GetHashCode() ^ fileInfo.CreationTime.GetHashCode();
	}

	/// <summary>
	/// resolves and processes the path; substitutes any '.' or '..'
	/// </summary>
	/// <param name="path">the path</param>
	/// <param name="path_base">the base path to be worked with</param>
	/// <returns>the resolved path</returns>
	public static string Resolve(string path, string? path_base = null)
	{
		path_base ??= WorkingDir.Content;

		IEnumerable<string> segments = from i in GetSegments(path) select path.Substring(i);
		StringBuilder builder = new(64);

		ParseSegments(segments.ToArray(), builder, path_base);

		return builder.ToString().TrimEnd(Path.DirectorySeparatorChar);
	}

	public static IEnumerable<IndexRange> GetSegments(string path)
	{
		int seg_start = 0;
		for (int i = 0; i < path.Length; i++)
		{
			if (seg_start < 0)
			{
				if (!path[i].IsDirectorySeparator())
					seg_start = i;

				continue;
			}

			if (path[i].IsDirectorySeparator())
			{
				yield return seg_start..i;
				seg_start = -1;
			}

			if (Path.GetInvalidPathChars().Contains(path[i]))
			{
				yield return seg_start..i;
				seg_start = -1;

				// invalid char, no reason to continue
				break;
			}
		}

		if (seg_start >= 0)
		{
			yield return seg_start..path.Length;
		}
	}

	public static string GetParent(string path)
	{
		path = path.TrimEnd('/', '\\');
		int index = Math.Max(path.LastIndexOf('/'), path.LastIndexOf('\\'));
		if (index < 0)
			return "";
		return path[..index];
	}

	public static string GetName(string path)
	{
		path = path.TrimEnd('/', '\\');
		int index = Math.Max(path.LastIndexOf('/'), path.LastIndexOf('\\'));
		if (index < 0)
			return "";
		return path[(index + 1)..];
	}

	public static string GetBaseName(string path)
	{
		// we use the name to get the last extension dot for when the path already
		// contains a dot e.g. 'G:/Shipper/.vs/something'
		string name = GetName(path);
		if (string.IsNullOrEmpty(name))
			return name;
		int extension_dot = name.LastIndexOf('.');
		if (extension_dot == -1)
			return name;
		return name[..extension_dot];
	}

	public static string GetExtension(string path)
	{
		string name = GetName(path);
		if (string.IsNullOrEmpty(name))
			return name;
		int extension_dot = name.LastIndexOf('.');
		if (extension_dot == -1)
			return name;
		return name[(extension_dot + 1)..];
	}

	private static void ParseSegments(string[] segments, StringBuilder builder, string path_base)
	{
		void append(string str) => builder.Append(str).Append(Path.DirectorySeparatorChar);

		for (int i = 0; i < segments.Length; i++)
		{
			if (segments[i] == ".")
			{
				append(path_base);
				continue;
			}

			if (segments[i] == "..")
			{
				int count = segments.CountContinues(s => s == "..", i + 1);

				// gets the n-th parent (e.g. three '..' in a row will get the third parent or the grand grand parent)
				for (int n = 0; n < count + 1; n++)
				{
					path_base = GetParent(path_base);
				}

				// already processed the next 'count' parents, skip
				i += count;

				append(path_base);
				continue;
			}

			append(segments[i]);
		}
	}


	public readonly FilePath Parent { get => new(GetParent(Content)); }
	public readonly string Name { get => GetName(Content); }

	/// <summary>
	/// the file/directory name without the extension
	/// </summary>
	public readonly string BaseName { get => GetBaseName(Content); }
	public readonly string Extension { get => GetExtension(Content); }
	public readonly bool Exists { get => Path.Exists(Content); }
	public readonly bool IsDirectory { get => Directory.Exists(Content); }
	public readonly bool IsFile { get => File.Exists(Content); }

	public readonly string Content;


	public static readonly FilePath Executable = new(AppContext.BaseDirectory); // TODO: fix for single-file apps
	public static readonly FilePath ParentDir = Executable.Parent;
	public static readonly FilePath WorkingDir = new(System.Environment.CurrentDirectory);
}
