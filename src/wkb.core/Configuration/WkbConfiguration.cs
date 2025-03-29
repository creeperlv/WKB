using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wkb.core.Configuration
{
	[Serializable]
	public class WkbConfiguration
	{
		public List<string> Prefixes = new List<string>();
		public string Title = "WKB";
	}
}
