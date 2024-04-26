using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Shipper.TUI.ITUI;

namespace Shipper.TUI;
internal interface ITUI
{
	abstract void Draw(TextWriter writer);

}

