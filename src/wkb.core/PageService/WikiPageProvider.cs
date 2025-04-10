using Markdig;
using Markdig.Syntax;
using Markdown.ColorCode;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using wkb.core.Configuration;

namespace wkb.core.PageService
{
	public class WikiPageProvider : IPageProvider
	{
		public WkbCore core;
		Templator templator;
		string ContentPath = "./wwwroot/";
		MarkdownPipeline pipeline;
		public WikiPageProvider(WkbCore core)
		{
			this.core = core;
			pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions()
				.UsePipeTables().UseGridTables().UseColorCode(HtmlFormatterType.Style).Build();
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

			var FileItemTemplate = templator.GetTemplate(TemplateFiles.NavBarFile);
			var FolderItemTemplate = templator.GetTemplate(TemplateFiles.NavBarFolder);
			var parent = fileInfo.Directory;
			if (parent is null)
			{
				return "Sorry, got lost.";
			}
			if (parent.Exists)
				return ListFolder(FileItemTemplate, FolderItemTemplate, parent);
			else if (parent.Parent is not null)
			{
				if (parent.Parent.Exists)
					return ListFolder(FileItemTemplate, FolderItemTemplate, parent.Parent);
				else
					return "Sorry, got lost.";
			}
			return "Sorry, got lost.";
		}

		private string ListFolder(string FileItemTemplate, string FolderItemTemplate, DirectoryInfo parent)
		{
			var dirs = parent.EnumerateDirectories();
			var files = parent.EnumerateFiles();
			StringBuilder sb = new StringBuilder();
			var compound = new ComposeCompound() { Variables = { { "ITEM_URL", "" }, { "ITEM_NAME", "" } } };
			if (parent.FullName != ContentPath)
			{
				compound.Variables["ITEM_URL"] = "../";
				compound.Variables["ITEM_NAME"] = "..";
				sb.Append(PageComposer.Compose(FolderItemTemplate, compound));
			}
			foreach (var dir in dirs)
			{
				var infoFile = Path.Combine(dir.FullName, ".info");
				if (File.Exists(infoFile))
				{
					compound.Variables["ITEM_URL"] = $"{dir.Name}/";
					compound.Variables["ITEM_NAME"] = File.ReadAllText(infoFile);
					sb.Append(PageComposer.Compose(FolderItemTemplate, compound));
				}
				else
				{
					compound.Variables["ITEM_URL"] = $"{dir.Name}/";
					compound.Variables["ITEM_NAME"] = dir.Name;
					sb.Append(PageComposer.Compose(FolderItemTemplate, compound));
				}
			}
			StringBuilder Files = new StringBuilder();
			foreach (var file in files)
			{
				if (file.Name.EndsWith(".info")) continue;
				if (!(file.Name.EndsWith(".md", StringComparison.InvariantCultureIgnoreCase) ||
					file.Name.EndsWith(".txt", StringComparison.InvariantCultureIgnoreCase)))
				{
					continue;
				}
				if (file.Name.StartsWith("index.", StringComparison.InvariantCultureIgnoreCase))
				{
					var infoFile = Path.Combine(parent.FullName, ".info");
					if (File.Exists(infoFile))
					{
						compound.Variables["ITEM_URL"] = $"{file.Name}";
						compound.Variables["ITEM_NAME"] = File.ReadAllText(infoFile);
						sb.Append(PageComposer.Compose(FileItemTemplate, compound));
					}
					else
					{
						compound.Variables["ITEM_URL"] = $"{file.Name}";
						compound.Variables["ITEM_NAME"] = parent.Name;
						sb.Append(PageComposer.Compose(FileItemTemplate, compound));
					}

				}
				else
				{
					var InfoFile = file.FullName + ".info";
					if (File.Exists(InfoFile))
					{
						compound.Variables["ITEM_URL"] = $"{file.Name}";
						compound.Variables["ITEM_NAME"] = File.ReadAllText(InfoFile);
						Files.Append(PageComposer.Compose(FileItemTemplate, compound));
					}
					else
					if (file.Name.EndsWith(".md", StringComparison.InvariantCultureIgnoreCase))
					{
						compound.Variables["ITEM_URL"] = $"{file.Name}";
						compound.Variables["ITEM_NAME"] = file.Name.Substring(0, file.Name.Length - 3);
						Files.Append(PageComposer.Compose(FileItemTemplate, compound));
					}
					else
					{

						compound.Variables["ITEM_URL"] = $"{file.Name}";
						compound.Variables["ITEM_NAME"] = file.Name;
						Files.Append(PageComposer.Compose(FileItemTemplate, compound));
					}
				}
			}
			sb.Append(Files);
			return sb.ToString();
		}
		public string ObtainPage(PageTarget target)
		{
			Stopwatch sw = Stopwatch.StartNew();
			ComposeCompound compound;
			string path = Path.Combine(ContentPath, target.ProcessedURL);
			if (Directory.Exists(path))
			{
				target.context.Response.Redirect("./index.md");
			}
			var content = templator.GetTemplate(TemplateFiles.wikiView, target.IsMobile);
			Trace.WriteLine(path);
			if (!File.Exists(path))
			{
				compound = new ComposeCompound()
				{
					Variables = new Dictionary<string, string>()
					{
						{ "TITLE","Not found!"},
						{ "URL_PATH",target.ProcessedURL},
						{"WIKI_CONTENT","Document not found!" },
						{"WIKI_NAVBAR",GenerateFolderStructure(target,path)}
					}
				};
				goto END;
			}
			FileInfo fi = new FileInfo(path);
			bool IsIndexFile = fi.Name.StartsWith("index", StringComparison.InvariantCultureIgnoreCase);
			var md = File.ReadAllText(path);
			string title = "";
			var infoFile = path + ".info";
			if (File.Exists(infoFile))
			{
				title = File.ReadAllText(infoFile);
			}
			else
			{
				if (IsIndexFile)
				{
					if (fi.Directory is not null)
					{
						var parentInfoFile = Path.Combine(fi.Directory.FullName, ".info");
						if (File.Exists(parentInfoFile))
						{
							title = File.ReadAllText(parentInfoFile);
							goto AFTER_TITLE;
						}
					}
				}
				title = fi.Name[..^3];
			}
		AFTER_TITLE:
			string l = Markdig.Markdown.ToHtml(md, pipeline);
			compound = new ComposeCompound()
			{
				Variables = new Dictionary<string, string>()
				{
					{ "TITLE",title},
					{ "URL_PATH",target.ProcessedURL},
					{ "CREATE_TIME",fi.CreationTime.ToLongDateString()},
					{ "MODIFY_TIME",fi.LastWriteTime.ToLongDateString()},
					{ "WIKI_CONTENT",l },
					{ "WIKI_NAVBAR",GenerateFolderStructure(target,path)}
				}
			};
		END:
			string pageContent = PageComposer.Compose(content, compound);
			TimeSpan elapsed = sw.Elapsed;
			sw.Stop();
			return PageComposer.Compose(pageContent, new ComposeCompound()
			{
				Variables = new Dictionary<string, string>()
				{
					{ "COMPOSE_TIME",$"{elapsed.TotalMilliseconds}"},
				}
			});
		}
	}
}
