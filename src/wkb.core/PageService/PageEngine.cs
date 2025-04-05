using Markdig;
using System.Diagnostics;
using System.Net;
using wkb.core.Configuration;

namespace wkb.core.PageService
{
	public class PageEngine
	{
		public ConfigurationService service;

		public PageEngine(ConfigurationService service)
		{
			this.service = service;
		}

		public Dictionary<PageTypes, IPageProvider> Providers = new Dictionary<PageTypes, IPageProvider>();
		public void RegisterProvider(PageTypes pageType, IPageProvider provider)
		{
			if (!Providers.TryAdd(pageType, provider))
			{
				Providers[pageType] = provider;
			}
		}
		public string ServeWikiPage(HttpListenerContext context, string path)
		{
			if (Providers.TryGetValue(PageTypes.WikiPage, out var provider))
			{
				return provider.ObtainPage(new PageTarget(context, path));
			}
			return "<html><body><h1>The server is not configured correctly";
		}
	}
	public class PageTarget
	{
		public HttpListenerContext context;
		public string ProcessedURL;

		public PageTarget(HttpListenerContext context, string processedURL)
		{
			this.context = context;
			ProcessedURL = processedURL;
		}
	}
	public interface IPageProvider
	{
		string ObtainPage(PageTarget target);
	}
	public class WikiPageProvider : IPageProvider
	{
		public WkbCore core;
		Templator templator;
		string ContentPath = "./wwwroot/";
		public WikiPageProvider(WkbCore core)
		{
			this.core = core;
			templator = new Templator(core);
			core.configurationService.OnApplyConfiguration.Add(Configure);
			Configure();
		}
		~WikiPageProvider()
		{
			if (core.configurationService.OnApplyConfiguration.Contains(Configure))
				core.configurationService.OnApplyConfiguration.Remove(Configure);
		}
		void Configure()
		{

		}
		public string ObtainPage(PageTarget target)
		{
			string path = Path.Combine(ContentPath, target.ProcessedURL);
			if (Directory.Exists(path))
			{
				target.context.Response.Redirect("./index.md");
			}
			var file = templator.FindFile(TemplateFiles.wikiDesktopView);
			var content = File.ReadAllText(file);
			Trace.WriteLine(path);
			if (!File.Exists(path))
			{

				return PageComposer.Compose(content, new ComposeCompound()
				{
					Variables = new Dictionary<string, string>()
				{
					{"WIKI_CONTENT","Document not found!" }
				}
				});
			}
			var md = File.ReadAllText(path);
			var l = Markdig.Markdown.Parse(md).ToHtml();
			return PageComposer.Compose(content, new ComposeCompound()
			{
				Variables = new Dictionary<string, string>()
				{
					{"WIKI_CONTENT",l }
				}
			});
		}
	}
}
