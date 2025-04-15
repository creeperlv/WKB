using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using wkb.core.Utilities;

namespace wkb.core.PageService
{
	public static class PageComposer
	{
		static char[] validateChars;
		public static bool UseNewComposer = true;
		internal static Dictionary<string, string> GlobalVariables = new();
		static PageComposer()
		{
			List<char> list = new List<char>();
			for (int i = 0; i < 26; i++)
			{
				list.Add((char)((int)'a' + i));
			}
			for (int i = 0; i < 26; i++)
			{
				list.Add((char)((int)'A' + i));
			}
			list.Add('_');
			validateChars = list.ToArray();
			
			GlobalVariables.Add("CPU_ARCH", RuntimeInformation.ProcessArchitecture.ToString());
			GlobalVariables.Add("DOTNET_FMWK", RuntimeInformation.FrameworkDescription);
			GlobalVariables.Add("CPU_COUNT", Environment.ProcessorCount.ToString());
			GlobalVariables.Add("SYS_ARCH", RuntimeInformation.OSArchitecture.ToString());
			GlobalVariables.Add("SYS_DESC", RuntimeInformation.OSDescription);
			GlobalVariables.Add("PLATFORM", Environment.OSVersion.Platform.ToString());
			GlobalVariables.Add("OS_VERSION", Environment.OSVersion.Version.ToString());
			GlobalVariables.Add("OS_SP", Environment.OSVersion.ServicePack.ToString());
			GlobalVariables.Add("OS_NAME", EnvironmentUtils.GetOSName());
			GlobalVariables.Add("PLAT_NAME", EnvironmentUtils.GetPlatformName());
			GlobalVariables.Add("RT_MODE", EnvironmentUtils.GetRuntimeMode());

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
							if (compound.TryGetVariable(key, out var value, ExposeEnvironmentVariables))
							{
								stringBuilder.Append(value);
								Index = i;
								goto END_ITERATION;
							}
							//if (compound.Variables.ContainsKey(key))
							//{
							//	stringBuilder.Append(compound.Variables[key]);
							//	Index = i;
							//	goto END_ITERATION;
							//}
							//if (ExposeEnvironmentVariables)
							//{
							//	var value = Environment.GetEnvironmentVariable(key);
							//	if (value is not null)
							//	{
							//		stringBuilder.Append(value);
							//		Index = i;
							//		goto END_ITERATION;
							//	}
							//}
							stringBuilder.Append(pageContent.AsSpan(id, i - id));
							Index = i;
							goto END_ITERATION;
						}
					}
					{
						var k2 = pageContent.Substring(id + 1);

						if (compound.TryGetVariable(k2, out var value, ExposeEnvironmentVariables))
						{
							stringBuilder.Append(compound.Variables[k2]);
							break;
						}
						//	if (compound.Variables.ContainsKey(k2))
						//{
						//	stringBuilder.Append(compound.Variables[k2]);
						//	break;
						//}
						//if (ExposeEnvironmentVariables)
						//{
						//	var value = Environment.GetEnvironmentVariable(k2);
						//	if (value is not null)
						//	{
						//		stringBuilder.Append(value);
						//		break;
						//	}
						//}
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
		public bool TryGetVariable(string key, [MaybeNullWhen(false)] out string result, bool useEnvironmentVariable = false)
		{
			if (Variables.TryGetValue(key, out result))
			{
				return true;
			}
			if (PageComposer.GlobalVariables.TryGetValue(key, out result))
			{
				return true;
			}
			if (useEnvironmentVariable)
			{
				result = Environment.GetEnvironmentVariable(key);
				return result != null;
			}
			result = null;
			return false;

		}
	}
	public enum PageTypes
	{
		WikiPage, ConfigurationHost, ConfigurationPage
	}
}
