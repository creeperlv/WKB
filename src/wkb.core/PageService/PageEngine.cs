using System.Net;

namespace wkb.core.PageService
{
	public class PageEngine
	{
		public Dictionary<PageTypes, IPageProvider> Providers = new Dictionary<PageTypes, IPageProvider>();
	}
	public interface IPageProvider
	{
		string ObtainPage(HttpListenerContext context);
	}
}
