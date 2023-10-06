using Dalamud.Configuration;
using System.Text.Json;

namespace MinimapMarkerMagnitude.Config;

public class Configuration : IPluginConfiguration
{
	public bool EnableResizing { get; set; } = true;

	public float DefaultMinimapIconScale { get; set; } = 1f;

	public bool ResizeOffMapIcons { get; set; } = false;

	public float DefaultOffMapIconScalar { get; set; } = 1f;

	public List<MinimapIconGroup> IconGroups { get; set; } = new();

	public int Version { get; set; } = 0;

	private const string FileName = "Config.json";

	public static void Load()
	{
		var loc = Path.Combine(Services.PluginInterface.GetPluginConfigDirectory(), FileName);
		if (File.Exists(loc))
		{
			Services.Config = JsonSerializer.Deserialize<Configuration>(File.ReadAllText(loc))!;
		}
		else
		{
			Services.Config = new Configuration();
			Services.Config.Save();
		}
	}

	public void Save()
	{
		File.WriteAllText(Path.Combine(Services.PluginInterface.GetPluginConfigDirectory(), FileName),
			JsonSerializer.Serialize(this, new JsonSerializerOptions
			{
				WriteIndented = true
			}));
		ResizeUtil.CompileIconSizes();
	}
}

public class MinimapIconGroup
{
	public string GroupName { get; set; } = null!;

	public HashSet<int> GroupIconIds { get; set; } = new();

	public float GroupScale { get; set; } = 1.0f;
}
