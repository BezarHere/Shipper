using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Shipper;

internal readonly struct FilePath
{

	public FilePath(string path)
	{
		ArgumentNullException.ThrowIfNull(path);
		this.Content = Path.GetFullPath(path);
	}

	public override string ToString()
	{
		return Content;
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

	public static IndexRange[] GetSegments(string path)
	{
		List<IndexRange> segments = new();

		int seg_start = 0;
		for (int i = 0; i < path.Length; i++)
		{
			if (seg_start < 0)
			{
				if (!(path[i] == '\\' || path[i] == '/'))
					seg_start = i;

				continue;
			}

			if (path[i] == '\\' || path[i] == '/')
			{
				segments.Add(new(seg_start, i));
				seg_start = -1;
			}

			if (Path.GetInvalidPathChars().Contains(path[i]))
			{
				segments.Add(new(seg_start, i));
				seg_start = -1;

				// invalid char, no reason to continue
				break;
			}
		}

		if (seg_start > 0)
		{
			segments.Add(new(seg_start, path.Length));
		}

		return segments.ToArray();
	}

	public static string GetParent(string path)
	{
		path = path.TrimEnd('/', '\\');
		int index = Math.Max(path.LastIndexOf('/'), path.LastIndexOf('\\'));
		if (index < 0)
			return "";
		return path[..index];
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

	public readonly FilePath Parent { get => new(FilePath.GetParent(Content)); }
	public readonly bool Exists { get => Path.Exists(Content); }
	public readonly bool IsDirectory { get => Directory.Exists(Content); }
	public readonly bool IsFile { get => File.Exists(Content); }

	public readonly string Content;


	public static readonly FilePath Current = new(Assembly.GetExecutingAssembly().Location);
	public static readonly FilePath BaseDir = Current.Parent;
	public static readonly FilePath WorkingDir = BaseDir; // TODO: find out how to get the working directory
}
