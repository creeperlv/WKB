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
			if (Configuration.OutputLogToConsole)
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
		public bool IsConfigured { get; set; } = false;
		public bool EnableAccessLog { get; set; } = true;
		public bool OutputLogToConsole { get; set; } = true;
		public List<string> Prefixes { get; set; } = ["127.0.0.1:8080"];
		public string Title { get; set; } = "WKB";
		public DataStore UserData { get; set; }
		public bool IsLocalizationEnabled { get; set; }
		public bool IsUsingGit { get; set; }
		public bool UsingGitCommandInsteadOfLibGit2Sharp { get; set; }
		public string KBPath { get; set; } = "./kb/";
	}
	public enum DataStore
	{
		NoDataBase,
		SQLite
	}
}
