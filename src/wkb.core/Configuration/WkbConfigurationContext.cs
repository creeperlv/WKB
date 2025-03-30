using System.Text.Json.Serialization;

namespace wkb.core.Configuration
{
	[JsonSerializable(typeof(WkbConfiguration))]
	internal partial class WkbConfigurationContext : JsonSerializerContext
	{
	}
}
