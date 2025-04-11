using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wkb.core.PageService
{
	public static class PageComposer
	{
		static char[] validateChars;
		public static bool UseNewComposer = true;
		static PageComposer()
		{
			validateChars = new char[52];
			for (int i = 0; i < 26; i++)
			{
				validateChars[i] = (char)((int)'a' + i);
			}
			for (int i = 0; i < 26; i++)
			{
				validateChars[26 + i] = (char)((int)'A' + i);
			}
		}
		public static string ComposeFile(string file, ComposeCompound compound, bool ExposeEnvironmentVariables = false)
		{
			return Compose(File.ReadAllText(file), compound, ExposeEnvironmentVariables);
		}
		public static string Compose(string pageContent, ComposeCompound compound, bool ExposeEnvironmentVariables = false)
		{
			if (UseNewComposer)
			{

				StringBuilder stringBuilder = new StringBuilder();
				int Index = 0;
				while (true)
				{
					var id = pageContent.IndexOf('$', Index);
					if (id <= -1)
					{
						if (Index < pageContent.Length)
						{
							stringBuilder.Append(pageContent.AsSpan(Index, pageContent.Length - Index));
						}
						break;
					}
					if (id > 0)
					{
						if (pageContent[id - 1] == '\\')
						{
							stringBuilder.Append(pageContent.AsSpan(Index, id - Index - 1));
							stringBuilder.Append('$');
							goto NEXT_ITERATION;
						}
					}
					if (id > Index)
					{
						stringBuilder.Append(pageContent.AsSpan(Index, id - Index));

					}
					for (int i = id + 1; i < pageContent.Length; i++)
					{
						if (!validateChars.Contains(pageContent[i]))
						{
							string key = pageContent.Substring(id + 1, i - id - 1);
							if (compound.Variables.ContainsKey(key))
							{
								stringBuilder.Append(compound.Variables[key]);
								Index = i;
								goto END_ITERATION;
							}
							if (ExposeEnvironmentVariables)
							{
								var value = Environment.GetEnvironmentVariable(key);
								if (value is not null)
								{
									stringBuilder.Append(value);
									Index = i;
									goto END_ITERATION;
								}
							}
							stringBuilder.Append(pageContent.AsSpan(id, i - id - 1));
							Index = i;
							goto END_ITERATION;
						}
					}
					{
						var k2 = pageContent.Substring(id + 1);
						if (compound.Variables.ContainsKey(k2))
						{
							stringBuilder.Append(compound.Variables[k2]);
							break;
						}
						if (ExposeEnvironmentVariables)
						{
							var value = Environment.GetEnvironmentVariable(k2);
							if (value is not null)
							{
								stringBuilder.Append(value);
								break;
							}
						}
						stringBuilder.Append('$');
						stringBuilder.Append(k2);
						break;
					}
				NEXT_ITERATION:
					Index = id + 1;
				END_ITERATION:;

				}
				return stringBuilder.ToString();
			}
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
