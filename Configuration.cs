using Dalamud.Configuration;
using Dalamud.Plugin;

namespace MinimapMarkerMagnitude;

[Serializable]
public class Configuration : IPluginConfiguration
{
	public int Version { get; set; } = 0;

	public bool EnableResizing { get; set; } = true;

	public float MinimapIconScale { get; set; } = 1.0f;

	public bool EnableOffMapResizing { get; set; } = false;

	public float OffMapIconScale { get; set; } = 1.0f;

	[NonSerialized]
	private DalamudPluginInterface? _pluginInterface;

	public void Initialize(DalamudPluginInterface pluginInterface)
	{
		_pluginInterface = pluginInterface;
	}

	public void Save()
	{
		_pluginInterface!.SavePluginConfig(this);
	}
}
