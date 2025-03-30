using wkb.core.HttpService;

namespace wkb.app;

class Program
{
	static void Main(string[] args)
	{
		var configService=new core.Configuration.ConfigurationService();
		HttpServer server = new HttpServer(configService);
		
		server.Start();
	}
}
