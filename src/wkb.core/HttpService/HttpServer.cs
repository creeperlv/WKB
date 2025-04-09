using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using wkb.core.ApiService;
using wkb.core.Configuration;
using wkb.core.PageService;

namespace wkb.core.HttpService
{
	public class HttpServer
	{
		public ConfigurationService configService;
		public WkbCore core;
		CancellationToken cancellationToken = new CancellationToken();
		public List<Func<HttpListenerContext, bool>> OnNewHttpRequest = new List<Func<HttpListenerContext, bool>>();
		public HttpServer(WkbCore core)
		{
			this.core = core;
			this.configService = core.configurationService;
			OnNewHttpRequest.Add((context) =>
			{
				if (this.configService.Configuration.TryGetConfig(WkbConfigurationKeys.EnableAccessLog, true))
					Trace.WriteLine($"{context.Request.RemoteEndPoint.Address}:{context.Request.RemoteEndPoint.Port}>{context.Request.HttpMethod}:{context.Request.Url}");
				return false;
			});
			OnNewHttpRequest.Add((context) =>
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

			});
			OnNewHttpRequest.Add(core.apiHub.Process);
			OnNewHttpRequest.Add(core.rfs.Process);
		}
		public void Start()
		{
			HttpListener httpListener = new();
			configService.Configuration.TryGetConfigAsList(WkbConfigurationKeys.Prefix).ForEach(x =>
			{
				if (!x.EndsWith("/"))
				{
					x += "/";
				}
				var X = x.ToUpper();
				if (X.StartsWith("HTTP:") || X.StartsWith("HTTPS:"))
				{
					httpListener.Prefixes.Add(x);
				}
				else
				{
					x = "http://" + x;
					httpListener.Prefixes.Add(x);
				}
			});
			httpListener.Start();
			while (true)
			{
				try
				{
					var task = httpListener.GetContextAsync();
					task.Wait(cancellationToken);
					var context = task.Result;
					if (context is not null)
					{
						Task.Run(() =>
						{
							try
							{
								foreach (var item in OnNewHttpRequest)
								{
									if (item(context))
									{
										break;
									}
								}
								context.Response.Close();
							}
							catch (Exception e)
							{
								Trace.WriteLine(e);
							}
							finally
							{
								try
								{
									context.Response.StatusCode = 500;
								}
								catch (Exception)
								{
								}
								context.Response.Close();
							}
						});
					}
				}
				catch (Exception e)
				{
					Trace.WriteLine(e);
				}
			}
		}
	}
}
