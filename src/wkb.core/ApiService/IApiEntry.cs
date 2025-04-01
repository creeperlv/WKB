using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using wkb.core.Configuration;

namespace wkb.core.ApiService
{
	public delegate void ApiEntry(HttpListenerContext context);
	public class ApiHub
	{
		public ConfigurationService ConfigurationService;

		public ApiHub(WkbCore core)
		{
			ConfigurationService = core.configurationService;
		}

		public Dictionary<string, ApiEntry> Apis = [];
		public void Register(string path, ApiEntry entry)
		{
			if (!Apis.TryAdd(path, entry))
			{
				Apis[path] = entry;
			}
		}
		public bool Process(HttpListenerContext context)
		{
			if (context.Request.Url?.LocalPath.StartsWith("/api/") ?? false)
			{

				var L0 = context.Request.Url.LocalPath["/api/".Length..];
				foreach (var k in Apis.Keys)
				{
					if (L0.StartsWith(k))
					{
						try
						{
							Apis[k](context);
						}
						catch (Exception e)
						{
							Trace.WriteLine(e);
						}
						break;
					}
				}
				return true;
			}
			return false;
		}
	}
	public class AuthenticationAPI
	{
		WkbCore core;
		public AuthenticationAPI(WkbCore core)
		{
			this.core = core;
		}
		public void SetupAll()
		{
			core.apiHub.Register("user/register", Register);
			core.apiHub.Register("user/login", Register);
		}
		public void Register(HttpListenerContext context)
		{
			Trace.WriteLine($"[{context.Request.RemoteEndPoint}][API][Authentication]Register");
		}
		public void Login(HttpListenerContext context)
		{
			Trace.WriteLine($"[{context.Request.RemoteEndPoint}][API][Authentication]Register");
		}
	}
}
