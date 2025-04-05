using System.Collections;
using wkb.core;
using wkb.core.HttpService;

namespace wkb.app;

class Program
{
	static void Main(string[] args)
	{
		var configService = new core.Configuration.ConfigurationService();
		for (int i = 0; i < args.Length; i++)
		{
			var item = args[i];
			switch (item)
			{
				case "--create-config-file":
					configService.Save();
					return;
					break;
				default:
					break;
			}
		}
		WkbCore core = new WkbCore(configService);

		core.Start();
	}
}
