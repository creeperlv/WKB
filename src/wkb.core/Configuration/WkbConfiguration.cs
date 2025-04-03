using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace wkb.core.Configuration
{
	public class ConfigurationService
	{
		public string configFileName = "wkb.conf";
		public string ConfigurationFile;
		public WkbConfiguration Configuration;
		string DetermineConfigurationFilePath()
		{
			if (File.Exists(configFileName))
			{
				return new FileInfo(configFileName).FullName;
			}
			{
				var file = (Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "wkb", configFileName));
				if (File.Exists(file))
				{
					return file;
				}
			}
			{
				var file = (Path.Combine("/etc/wkb", configFileName));
				if (File.Exists(file))
				{
					return file;
				}
			}
			return configFileName;
		}
		public ConfigurationService()
		{
			ConfigurationFile = DetermineConfigurationFilePath();
			if (File.Exists(ConfigurationFile))
			{
				try
				{
					Configuration = JsonSerializer.Deserialize(File.ReadAllText(ConfigurationFile), typeof(WkbConfiguration), new WkbConfigurationContext()) as WkbConfiguration ?? new WkbConfiguration();
				}
				catch (Exception)
				{
					Configuration = new WkbConfiguration();

				}
			}
			else
			{
				Configuration = new WkbConfiguration();
			}
			Apply();
		}
		public void Apply()
		{
			if (Configuration.TryGetConfig<bool>(WkbConfigurationKeys.OutputLogToConsole, true))
			{
				Trace.Listeners.Add(new ConsoleTraceListener());
			}
		}
		public void Save()
		{
			if (File.Exists(ConfigurationFile))
			{
				File.Delete(ConfigurationFile);
			}
			File.WriteAllText(configFileName, JsonSerializer.Serialize(Configuration, typeof(WkbConfiguration), new WkbConfigurationContext()));
		}
	}
	[Serializable]
	public class WkbConfiguration
	{
		public Dictionary<string, string> Configuration { get; set; } = new Dictionary<string, string>(){
			{ WkbConfigurationKeys.Prefix,"127.0.0.1:8080"}
			};
		public string TryGetConfig(string key, string fallback = "")
		{
			if (Configuration.TryGetValue(key, out var value))
			{
				return value;
			}
			return fallback;
		}
		public List<string> TryGetConfigAsList(string key)
		{
			if (Configuration.TryGetValue(key, out var value))
			{
				var seg = value.Split(',');
				var L = new List<string>(seg.Length);
				foreach (var kv in seg)
				{
					L.Add(kv.Trim());
				}
				return L;
			}
			return [];
		}
		public T TryGetConfig<T>(string key, T fallback) where T : IParsable<T>
		{
			if (Configuration.TryGetValue(key, out var value))
			{
				if (T.TryParse(value, null, out var result))
				{
					return result;
				}
			}
			return fallback;
		}
	}
	public enum DataStore
	{
		NoDataBase,
		SQLite
	}
}
