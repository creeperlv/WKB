using System.Collections;
using wkb.core;
using wkb.core.HttpService;

namespace wkb.app;

class Program
{
	static void Main(string[] args)
	{
		var configService = new core.Configuration.ConfigurationService();
		WkbCore core = new WkbCore(configService);

		core.Start();
	}
}
