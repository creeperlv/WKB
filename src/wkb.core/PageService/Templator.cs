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
			basePath = this.core.configurationService.Configuration.TryGetConfig(WkbConfigurationKeys.TemplateLocation, Environment.ProcessPath ?? ".");
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
