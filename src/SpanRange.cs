using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shipper;
readonly struct SpanRange(int start, int end)
{
	public readonly int Start = start;
	public readonly int End = end;
	public readonly int Length { get => End - Start; }

	/// <summary>
	/// valid ranges have non-zero, positive length
	/// </summary>
	public readonly bool Valid { get => Length > 0; }
}
