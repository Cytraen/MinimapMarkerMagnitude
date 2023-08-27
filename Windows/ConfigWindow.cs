using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Numerics;

namespace MinimapMarkerMagnitude.Windows;

public class ConfigWindow : Window, IDisposable
{
	private Plugin Plugin;
	private Configuration Config;

	public ConfigWindow(Plugin plugin) : base(
		"Minimap Marker Magnitude Settings",
		ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
	{
		Size = new Vector2(0, 0);
		Plugin = plugin;
		Config = plugin.Config;
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
	}

	public override void Draw()
	{
		var enableResizing = Config.EnableResizing;
		var iconScale = (int)(Config.MinimapIconScale * 100);

		var changed = false;

		if (changed |= ImGui.Checkbox($"Resize Minimap Markers{new string(' ', 24)}", ref enableResizing))
		{
			Config.EnableResizing = enableResizing;
			if (!enableResizing)
			{
				Plugin.ResizeIcons(true);
			}
		}
		if (enableResizing)
		{
			if (changed |= ImGui.SliderInt("Icon Scale Percent", ref iconScale, 25, 200))
			{
				Config.MinimapIconScale = iconScale / 100.0f;
			}
		}
		if (changed)
		{
			Config.Save();
		}
	}
}
