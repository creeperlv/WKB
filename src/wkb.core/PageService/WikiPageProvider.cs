using Markdig;
using Markdig.Syntax;
using System.Diagnostics;
using System.Text;
using wkb.core.Configuration;

namespace wkb.core.PageService
{
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
			if (Environment.ProcessPath is not null)
			{
				FileInfo fi = new FileInfo(Environment.ProcessPath);

				ContentPath = this.core.configurationService.Configuration.TryGetConfig(WkbConfigurationKeys.ContentLocation, Path.Combine(fi.Directory?.FullName ?? ".", "wwwroot"));
			}
			else
				ContentPath = this.core.configurationService.Configuration.TryGetConfig(WkbConfigurationKeys.ContentLocation, "./wwwroot");
			DirectoryInfo di = new DirectoryInfo(ContentPath);
			ContentPath = di.FullName;
		}
		string GenerateFolderStructure(PageTarget target, string path)
		{
			FileInfo fileInfo = new FileInfo(path);

			var parent = fileInfo.Directory;
			if (parent is null)
			{
				return "Sorry, got lost.";
			}
			var dirs = parent.EnumerateDirectories();
			var files = parent.EnumerateFiles();
			StringBuilder sb = new StringBuilder();
			if (parent.FullName != ContentPath)
			{
				sb.Append($"<a href=\"../\"/><li><span class=\"iconify\" data-icon=\"mdi-folder-outline\"></span>..</li></a>");

			}
			foreach (var dir in dirs)
			{
				var infoFile = Path.Combine(dir.FullName, ".info");
				if (File.Exists(infoFile))
				{
					sb.Append($"<a href=\"{dir.Name}/\"/><li><span class=\"iconify\" data-icon=\"mdi-folder-outline\"></span>{File.ReadAllText(infoFile)}</li></a>");
				}
				else
				{
					sb.Append($"<a href=\"{dir.Name}/\"/><li><span class=\"iconify\" data-icon=\"mdi-folder-outline\"></span>{dir.Name}</li></a>");
				}
			}
			foreach (var file in files)
			{
				if (file.Name == ".info") continue;
				if (file.Name.StartsWith("index.", StringComparison.InvariantCultureIgnoreCase))
				{
					var infoFile = Path.Combine(parent.FullName, ".info");
					if (File.Exists(infoFile))
					{
						sb.Append($"<a href=\"{file.Name}\"/><li>{File.ReadAllText(infoFile)}</li></a>");
					}
					else
					{
						sb.Append($"<a href=\"{file.Name}\"/><li>{parent.Name}</li></a>");
					}

				}
				else
				{
					if (file.Name.EndsWith(".md", StringComparison.InvariantCultureIgnoreCase))
					{
						sb.Append($"<a href=\"{file.Name}\"/><li>{file.Name.Substring(0, file.Name.Length - 3)}</li></a>");
					}
					else
						sb.Append($"<a href=\"{file.Name}\"/><li>{file.Name}</li></a>");
				}
			}
			return sb.ToString();
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
					{"WIKI_CONTENT","Document not found!" },
					{"WIKI_NAVBAR",GenerateFolderStructure(target,path)}
				}
				});
			}
			var md = File.ReadAllText(path);
			MarkdownDocument markdownDocument = Markdown.Parse(md);

			string l = markdownDocument.ToHtml();

			return PageComposer.Compose(content, new ComposeCompound()
			{
				Variables = new Dictionary<string, string>()
				{
					{ "TITLE",md.Substring(markdownDocument.First().Span.Start,markdownDocument.First().Span.Length).TrimStart('#').Trim()},
					{"WIKI_CONTENT",l },
					{"WIKI_NAVBAR",GenerateFolderStructure(target,path)}
				}
			});
		}
	}
}
