using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using wkb.core.Configuration;

namespace wkb.core.HttpService
{
	public class HttpServer
	{
		public ConfigurationService configService;
		CancellationToken cancellationToken = new CancellationToken();
		public List<Func<HttpListenerContext, bool>> OnNewHttpRequest = new List<Func<HttpListenerContext, bool>>();
		public HttpServer(ConfigurationService configService)
		{
			this.configService = configService;
			OnNewHttpRequest.Add((context) =>
			{
				if (this.configService.Configuration.EnableAccessLog)
					Trace.WriteLine($"{context.Request.RemoteEndPoint.Address}:{context.Request.RemoteEndPoint.Port}>{context.Request.HttpMethod}:{context.Request.Url}");
				return false;
			});
			OnNewHttpRequest.Add((context) =>
			{
				if (!configService.Configuration.IsConfigured)
				{
					if (context.Request.Url?.LocalPath.StartsWith("/firstSetup") ?? false)
					{

					}
					else
						context.Response.Redirect("/firstSetup");

				}
				return true;
			});
		}
		public void Start()
		{
			HttpListener httpListener = new HttpListener();
			configService.Configuration.Prefixes.ForEach(x =>
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
						foreach (var item in OnNewHttpRequest)
						{
							if (item(context))
							{
								break;
							}
						}
						context.Response.Close();
					}
				}
				catch (Exception)
				{
					break;
				}
			}
		}
	}
}
