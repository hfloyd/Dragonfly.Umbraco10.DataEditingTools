
namespace Dragonfly.DataEditingTools;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Extensions;

internal static class ConfigReaderHelper
{
	public static ConfigData GetConfig(IConfiguration AppSettingsConfig)
	{
		var config = new ConfigData();
		AppSettingsConfig.GetSection(ConfigData.ConfigSection).Bind(config);
		return config;
	}

	public static string MapPath(string VirtualPath, bool IsInWwwRoot, IWebHostEnvironment HostingEnvironment)
	{
		if (IsInWwwRoot)
		{
			return HostingEnvironment.MapPathWebRoot(VirtualPath);
		}
		else
		{
			return HostingEnvironment.MapPathContentRoot(VirtualPath);
		}
	}


}


