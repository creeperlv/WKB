using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace wkb.core.Utilities
{
	public static class EnvironmentUtils
	{
		public static string GetOSName()
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				return "Windows";
			}
			else
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				return "Linux";
			}
			if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
			{
				return "BSD";
			}
			if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				return "OSX";
			}
			return "Unknown";
		}
		public static string GetPlatformName()
		{
			if (OperatingSystem.IsWindows())
			{
				return "Windows";
			}
			else
			if (OperatingSystem.IsWasi())
			{
				return "Wasi";
			}
			else
			if (OperatingSystem.IsMacOS())
			{
				return "macOS";
			}
			else
			if (OperatingSystem.IsMacCatalyst())
			{
				return "macOS(Catalyst)";
			}
			else
			if (OperatingSystem.IsAndroid())
			{
				return "Android";
			}
			else
			if (OperatingSystem.IsIOS())
			{
				return "iOS";
			}
			else
			if (OperatingSystem.IsTvOS())
			{
				return "tvOS";
			}
			else
			if (OperatingSystem.IsWatchOS())
			{
				return "watchOS";
			}
			else
			if (OperatingSystem.IsLinux())
			{
				return "Linux";
			}
			else
			if (OperatingSystem.IsBrowser())
			{
				return "Browser";
			}
			else
			if (OperatingSystem.IsFreeBSD())
			{
				return "FreeBSD";
			}
			return "Unknown";
		}
		public static string GetRuntimeMode()
		{
			if (RuntimeFeature.IsDynamicCodeSupported && RuntimeFeature.IsDynamicCodeCompiled)
			{
				return "JIT";
			}
			else
			if (RuntimeFeature.IsDynamicCodeSupported && !RuntimeFeature.IsDynamicCodeCompiled)
			{
				return "Interpret";
			}
			else
			if (!RuntimeFeature.IsDynamicCodeSupported && !RuntimeFeature.IsDynamicCodeCompiled)
			{
				return "AOT";
			}
			return "Unknown Runtime Mode";
		}
	}
}
