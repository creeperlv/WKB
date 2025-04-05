using System.Buffers;
using System.Diagnostics;
using System.Net;
using wkb.core.Configuration;

namespace wkb.core.HttpService
{
	public class RawFileService
	{
		public WkbCore core;

		string ContentPath = "./wwwroot/";
		int bufferSize = 4096;
		public RawFileService(WkbCore core)
		{
			this.core = core;
			core.configurationService.OnApplyConfiguration.Add(Configure);
			Configure();
		}
		~RawFileService()
		{
			if (core.configurationService.OnApplyConfiguration.Contains(Configure))
				core.configurationService.OnApplyConfiguration.Remove(Configure);
		}
		void Configure()
		{
			if (Environment.ProcessPath is not null)
			{
				FileInfo fi = new FileInfo(Environment.ProcessPath);

				ContentPath = this.core.configurationService.Configuration.TryGetConfig(WkbConfigurationKeys.ContentLocation, Path.Combine(fi.Directory?.FullName ?? ".", "wwwroot"));
			}
			else
				ContentPath = this.core.configurationService.Configuration.TryGetConfig(WkbConfigurationKeys.ContentLocation, "./wwwroot");
			if (int.TryParse(this.core.configurationService.Configuration.TryGetConfig(WkbConfigurationKeys.RawFileBufferSize, "4096"), out var data))
			{
				bufferSize = data;
			}
			DirectoryInfo di = new DirectoryInfo(ContentPath);
			ContentPath = di.FullName;
		}
		public bool Process(HttpListenerContext context)
		{
			if (context.Request.Url?.LocalPath.StartsWith("/raw/") ?? false)
			{
				var path = context.Request.Url?.LocalPath["/raw/".Length..];
				if (path is null)
				{
					context.Response.StatusCode = 404;
					context.Response.Close();
					return true;
				}
				path = Path.Combine(ContentPath, path);
				if (!File.Exists(path))
				{
					context.Response.StatusCode = 404;
					context.Response.Close();
					return true;
				}
				using var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
				context.Response.ContentLength64 = fs.Length;
				if (MimeTypes.TryGetMimeType(path, out var mimeType))
				{
					context.Response.ContentType = mimeType;
				}
				byte[] buffer = ArrayPool<byte>.Shared.Rent(this.bufferSize);
				int S = 0;
				while (true)
				{
					var l = fs.Read(buffer);
					if (l <= 0)
					{
						break;
					}
					S += buffer.Length;
					context.Response.OutputStream.Write(buffer, 0, l);
				}
				return true;
			}
			return false;
		}
	}
}
