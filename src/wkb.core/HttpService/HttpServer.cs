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
			OnNewHttpRequest.Add(core.contentService.Process);
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
