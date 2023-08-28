using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Numerics;

namespace MinimapMarkerMagnitude.Windows;

internal class ConfigWindow : Window, IDisposable
{
	private readonly Configuration _config;
	private readonly AddonNaviMapUpdateHook _addonHook;

	internal ConfigWindow(Configuration config, AddonNaviMapUpdateHook addonHook) : base(
		"Minimap Marker Magnitude Settings",
		ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
	{
		Size = new Vector2(0, 0);
		_config = config;
		_addonHook = addonHook;
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
	}

	public override void Draw()
	{
		var enableResizing = _config.EnableResizing;
		var iconScale = (int)(_config.MinimapIconScale * 100);

		var changed = false;

		if (changed |= ImGui.Checkbox($"Resize Minimap Markers{new string(' ', 24)}", ref enableResizing))
		{
			_config.EnableResizing = enableResizing;
			if (enableResizing)
			{
				_addonHook.Enable();
			}
			else
			{
				_addonHook.Disable();
			}
		}
		if (enableResizing)
		{
			if (changed |= ImGui.SliderInt("Icon Scale Percent", ref iconScale, 25, 200))
			{
				_config.MinimapIconScale = iconScale / 100.0f;
			}
		}
		if (changed)
		{
			_config.Save();
		}
	}
}
