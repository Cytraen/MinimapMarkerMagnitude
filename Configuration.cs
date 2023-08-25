using Dalamud.Configuration;
using Dalamud.Plugin;

namespace MinimapMarkerModifier
{
	[Serializable]
	public class Configuration : IPluginConfiguration
	{
		public int Version { get; set; } = 0;

		public bool EnableResizing { get; set; } = true;

		public float MinimapIconScale { get; set; } = 1.0f;

		[NonSerialized]
		private DalamudPluginInterface? PluginInterface;

		public void Initialize(DalamudPluginInterface pluginInterface)
		{
			PluginInterface = pluginInterface;
		}

		public void Save()
		{
			PluginInterface!.SavePluginConfig(this);
		}
	}
}
