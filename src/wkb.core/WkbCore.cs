using wkb.core.ApiService;
using wkb.core.Configuration;
using wkb.core.HttpService;
using wkb.core.PageService;

namespace wkb.core;

public class WkbCore
{
	public ConfigurationService configurationService;
	public ApiHub apiHub;
	public PageEngine pageEngine;
	public RawFileService rfs;
	public ContentService contentService;
	public WkbCore(ConfigurationService configurationService)
	{
		this.configurationService = configurationService;
		this.apiHub = new ApiHub(this);
		AuthenticationAPI api = new AuthenticationAPI(this);
		api.SetupAll();
		pageEngine = new PageEngine(this.configurationService);
		rfs = new RawFileService(this);
		contentService = new ContentService(this);
		pageEngine.RegisterProvider(PageTypes.WikiPage, new WikiPageProvider(this));
	}
	public void Start()
	{
		HttpServer server = new HttpServer(this);
		server.Start();
	}
}
