using System.Net;
using wkb.core.Configuration;

namespace wkb.core.HttpService
{
	public class HistoryService
	{
		public WkbCore core;
		ConfigurationService configService;
		public HistoryService(WkbCore core)
		{
			this.core = core;
			configService = core.configurationService;
		}
		void realProcess(HttpListenerContext context, string path)
		{

		}
		public bool Process(HttpListenerContext context)
		{
			if (context.Request.Url?.LocalPath.StartsWith("/history/", StringComparison.InvariantCultureIgnoreCase) ?? false)
			{
				var path = context.Request.Url?.LocalPath["/history/".Length..];
				realProcess(context, path ?? "index.md");
				return true;
			}
			if (context.Request.Url?.LocalPath.StartsWith("/m-history/", StringComparison.InvariantCultureIgnoreCase) ?? false)
			{

				var path = context.Request.Url?.LocalPath["/m-history/".Length..];
				realProcess(context, path ?? "index.md");
				return true;
			}
			return false;
		}
	}
}
