using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Globalization;
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
		var iconScale = _config.MinimapIconScale * 100f;
		var enableOffMapResizing = _config.EnableOffMapResizing;
		var offMapIconScale = _config.OffMapIconScale * 100f;

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
			if (changed |= Slider("Marker Scale", ref iconScale, 25f, 200f))
			{
				_config.MinimapIconScale = iconScale / 100f;
			}

			if (changed |= ImGui.Checkbox($"Resize Off-map Markers", ref enableOffMapResizing))
			{
				_config.EnableOffMapResizing = enableOffMapResizing;
			}

			if (enableOffMapResizing)
			{
				if (changed |= Slider("Off-map Marker Scale", ref offMapIconScale, 25f, 200f))
				{
					_config.OffMapIconScale = offMapIconScale / 100f;
				}
			}
		}

		if (changed)
		{
			_config.Save();
		}
	}

	private static bool Slider(string label, ref float value, float min, float max)
	{
		return ImGui.SliderFloat(label, ref value, min, max,
			value.ToString(@".0\%\%", CultureInfo.CurrentCulture));
	}
}
