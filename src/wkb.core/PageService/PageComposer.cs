using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wkb.core.PageService
{
	public static class PageComposer
	{
		public static string ComposeFile(string file, ComposeCompound compound, bool ExposeEnvironmentVariables = false)
		{
			return Compose(File.ReadAllText(file), compound, ExposeEnvironmentVariables);
		}
		public static string Compose(string pageContent, ComposeCompound compound, bool ExposeEnvironmentVariables = false)
		{
			foreach (var item in compound.Variables.Keys)
			{
				pageContent = pageContent.Replace($"${item}", compound.Variables[item]);
			}
			if (ExposeEnvironmentVariables)
			{
				var ENV = Environment.GetEnvironmentVariables();
				foreach (var item in ENV.Keys)
				{
					pageContent = pageContent.Replace($"${item}", ENV[item]?.ToString() ?? "");
				}
			}
			return pageContent;
		}
	}
	public class ComposeCompound
	{
		public Dictionary<string, string> Variables = [];
		public string TryGetVariable(string key, bool useEnvironmentVariable = false)
		{
			if (Variables.TryGetValue(key, out var vv))
			{
				return vv;
			}
			if (useEnvironmentVariable)
			{
				return Environment.GetEnvironmentVariable(key) ?? key;
			}
			return key;
		}
	}
	public enum PageTypes
	{
		WikiPage, ConfigurationHost, ConfigurationPage
	}
}
