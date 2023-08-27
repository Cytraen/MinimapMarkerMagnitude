using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Numerics;

namespace MinimapMarkerModifier.Windows;

public class ConfigWindow : Window, IDisposable
{
	private Plugin Plugin;
	private Configuration Config;

	public ConfigWindow(Plugin plugin) : base(
		"Minimap Marker Modifier Settings",
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
		var iconScale = Config.MinimapIconScale;

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
			if (changed |= ImGui.SliderFloat("Icon Scale", ref iconScale, 0.25f, 2.0f))
			{
				Config.MinimapIconScale = iconScale;
			}
		}
		if (changed)
		{
			Config.Save();
		}
	}
}
