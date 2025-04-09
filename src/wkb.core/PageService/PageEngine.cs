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
		public string ServeWikiPage(HttpListenerContext context, string path,bool IsMobile)
		{
			if (Providers.TryGetValue(PageTypes.WikiPage, out var provider))
			{
				return provider.ObtainPage(new PageTarget(context, path,IsMobile));
			}
			return "<html><body><h1>The server is not configured correctly";
		}
	}
	public class PageTarget
	{
		public HttpListenerContext context;
		public string ProcessedURL;
		public bool IsMobile = false;
		public PageTarget(HttpListenerContext context, string processedURL, bool isMobile) : this(context, processedURL)
		{
			IsMobile = isMobile;
		}
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
}
