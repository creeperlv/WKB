using System.Diagnostics;
using wkb.core.Configuration;

namespace wkb.core.PageService
{
	internal class Templator
	{
		WkbCore core;
		string basePath = ".";
		internal Templator(WkbCore core)
		{
			this.core = core;
			this.core.configurationService.OnApplyConfiguration.Add(Configure);
			Configure();
		}
		~Templator()
		{
			this.core.configurationService.OnApplyConfiguration.Remove(Configure);
		}
		void Configure()
		{
			if (Environment.ProcessPath is not null)
			{
				FileInfo di = new FileInfo(Environment.ProcessPath);

				basePath = this.core.configurationService.Configuration.TryGetConfig(WkbConfigurationKeys.TemplateLocation, Path.Combine(di.Directory?.FullName ?? ".", "Templates"));
			}
			else
				basePath = this.core.configurationService.Configuration.TryGetConfig(WkbConfigurationKeys.TemplateLocation, "./Templates");
		}
		public string FindFile(string name)
		{
			return Path.Combine(basePath, name);
		}
	}
	internal static class TemplateFiles
	{
		internal const string wikiDesktopView = "wikiDesktopView.template";
	}
}
