using System.Net;
using System.Text;
using wkb.core.Configuration;

namespace wkb.core.HttpService
{
	public class ContentService
	{
		public WkbCore core;
		ConfigurationService configService;
		public ContentService(WkbCore core)
		{
			this.core = core;
			configService = core.configurationService;
		}

		public bool Process(HttpListenerContext context)
		{
			if (!bool.TryParse(configService.Configuration.TryGetConfig(WkbConfigurationKeys.EnableMobileTemplate, "true"), out var useMobile))
			{
				useMobile = true;
			}
			if (context.Request.Url?.LocalPath == "/")
			{
				if (!this.configService.Configuration.TryGetConfig(WkbConfigurationKeys.IsConfigured, false) && this.configService.Configuration.TryGetConfig(WkbConfigurationKeys.AdvancedServer, false))
				{
					if (context.Request.Url?.LocalPath.StartsWith("/firstSetup") ?? false)
					{

						return true;
					}
					else
						context.Response.Redirect("/firstSetup");
					return true;
				}
				if (useMobile && (context.Request.UserAgent?.IndexOf("Mobile", StringComparison.InvariantCultureIgnoreCase) >= 0))
				{
					context.Response.Redirect("/m-content/index.md");
				}
				else
					context.Response.Redirect("/content/index.md");
				return true;
			}
			if (context.Request.Url?.LocalPath.StartsWith("/content/") ?? false)
			{
				if (!this.configService.Configuration.TryGetConfig(WkbConfigurationKeys.IsConfigured, false) && this.configService.Configuration.TryGetConfig(WkbConfigurationKeys.AdvancedServer, false))
				{
					if (context.Request.Url?.LocalPath.StartsWith("/firstSetup") ?? false)
					{
						return true;
					}
					else
						context.Response.Redirect("/firstSetup");
					return true;
				}
				var path = context.Request.Url?.LocalPath["/content/".Length..];
				try
				{
					var content = core.pageEngine.ServeWikiPage(context, path ?? "index.md", false);
					var data = Encoding.UTF8.GetBytes(content);
					context.Response.ContentEncoding = Encoding.UTF8;
					context.Response.OutputStream.Write(data);
				}
				catch (Exception e)
				{
					var data = Encoding.UTF8.GetBytes($"<html><body><h1>Internal Server Error</h1><p>{e}</p>");
					context.Response.ContentEncoding = Encoding.UTF8;
					context.Response.OutputStream.Write(data);
				}
				context.Response.OutputStream.Flush();
				context.Response.OutputStream.Close();
				return true;
			}
			else
			if (context.Request.Url?.LocalPath.StartsWith("/m-content/") ?? false)
			{
				if (!this.configService.Configuration.TryGetConfig(WkbConfigurationKeys.IsConfigured, false) && this.configService.Configuration.TryGetConfig(WkbConfigurationKeys.AdvancedServer, false))
				{
					if (context.Request.Url?.LocalPath.StartsWith("/firstSetup") ?? false)
					{
						return true;
					}
					else
						context.Response.Redirect("/firstSetup");
					return true;
				}
				var path = context.Request.Url?.LocalPath["/m-content/".Length..];
				try
				{
					var content = core.pageEngine.ServeWikiPage(context, path ?? "index.md", true && useMobile);
					var data = Encoding.UTF8.GetBytes(content);
					context.Response.ContentEncoding = Encoding.UTF8;
					context.Response.OutputStream.Write(data);
				}
				catch (Exception e)
				{
					var data = Encoding.UTF8.GetBytes($"<html><body><h1>Internal Server Error</h1><p>{e}</p>");
					context.Response.ContentEncoding = Encoding.UTF8;
					context.Response.OutputStream.Write(data);
				}
				context.Response.OutputStream.Flush();
				context.Response.OutputStream.Close();
				return true;
			}
			return false;


		}
	}
}
