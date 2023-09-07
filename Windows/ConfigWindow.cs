using Dalamud.Interface.Components;
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
		var iconScale = (float)(Math.Pow(_config.MinimapIconScale, 2f) * 100f);

		var resizeOffMapIcons = _config.ResizeOffMapIcons;
		var offMapIconScale = (float)(Math.Pow(_config.OffMapIconScalar * 0.625f, 2f) * 100f);

		var overridePlayerMarker = _config.OverridePlayerMarker;
		var playerMarkerScale = (float)(Math.Pow(_config.PlayerMarkerScale, 2f) * 100f);

		var overrideAllyMarkers = _config.OverrideAllyMarkers;
		var allyMarkerScale = (float)(Math.Pow(_config.AllyMarkerScale, 2f) * 100f);

		var changed = false;

		if (changed |= ImGui.Checkbox("Resize Minimap Markers", ref enableResizing))
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

		if (!enableResizing)
		{
			ImGui.SameLine(0, 0);
			ImGui.Text(new string(' ', 30));
		}
		else
		{
			if (changed |= Slider("Marker Scale", ref iconScale, 5f, 400f))
			{
				_config.MinimapIconScale = float.Sqrt(iconScale / 100f);
			}

			if (changed |= ImGui.Checkbox("Resize Off-map Markers", ref resizeOffMapIcons))
			{
				_config.ResizeOffMapIcons = resizeOffMapIcons;
			}
			ImGuiComponents.HelpMarker("Enables changing the size of far away things like active quests and party members," +
									   "\nrelative to the marker scale.");

			if (resizeOffMapIcons)
			{
				if (changed |= Slider("Off-map Marker Scalar", ref offMapIconScale, 5f, 200f))
				{
					_config.OffMapIconScalar = float.Sqrt(offMapIconScale / 100f) / 0.625f;
				}
				ImGuiComponents.HelpMarker("By default, off-map markers are 39.1% the size of markers that are in minimap radius." +
										   "\nChanging this to 100% will make off-map markers the same size as markers that are in range.");
			}

			if (changed |= ImGui.Checkbox("Override Player Marker Scale", ref overridePlayerMarker))
			{
				_config.OverridePlayerMarker = overridePlayerMarker;
			}

			if (overridePlayerMarker)
			{
				if (changed |= Slider("Player Marker Scale", ref playerMarkerScale, 5f, 400f))
				{
					_config.PlayerMarkerScale = float.Sqrt(playerMarkerScale / 100f);
				}
			}

			if (changed |= ImGui.Checkbox("Override Ally Marker Scale", ref overrideAllyMarkers))
			{
				_config.OverrideAllyMarkers = overrideAllyMarkers;
			}

			if (overrideAllyMarkers)
			{
				if (changed |= Slider("Ally Marker Scale", ref allyMarkerScale, 5f, 400f))
				{
					_config.AllyMarkerScale = float.Sqrt(allyMarkerScale / 100f);
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
			value.ToString(@".0\%\%", CultureInfo.CurrentCulture), ImGuiSliderFlags.Logarithmic);
	}
}
