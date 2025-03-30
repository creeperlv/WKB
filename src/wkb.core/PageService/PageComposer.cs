using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wkb.core.PageService
{
	public class PageComposer
	{
		public PageComposer() { }
		public string ComposeFile(string file, bool ExposeEnvironmentVariables = false)
		{
			return Compose(File.ReadAllText(file), ExposeEnvironmentVariables);
		}
		public string Compose(string pageContent, bool ExposeEnvironmentVariables = false)
		{
			return pageContent;
		}
	}
}
