using Dalamud.Interface.Components;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Globalization;
using System.Numerics;

namespace MinimapMarkerMagnitude.Windows;

internal class ConfigWindow : Window, IDisposable
{
	private readonly Plugin _plugin;

	internal ConfigWindow(Plugin plugin) : base(
		"Minimap Marker Magnitude Settings",
		ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
	{
		_plugin = plugin;
		Size = new Vector2(0, 0);
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
	}

	public override void Draw()
	{
		var enableResizing = Services.Config.EnableResizing;
		var iconScale = (float)(Math.Pow(Services.Config.MinimapIconScale, 2f) * 100f);

		var resizeOffMapIcons = Services.Config.ResizeOffMapIcons;
		var offMapIconScale = (float)(Math.Pow(Services.Config.OffMapIconScalar * 0.625f, 2f) * 100f);

		var overridePlayerMarker = Services.Config.OverridePlayerMarker;
		var playerMarkerScale = (float)(Math.Pow(Services.Config.PlayerMarkerScale, 2f) * 100f);

		var overrideAllyMarkers = Services.Config.OverrideAllyMarkers;
		var allyMarkerScale = (float)(Math.Pow(Services.Config.AllyMarkerScale, 2f) * 100f);

		var changed = false;

		if (changed |= ImGui.Checkbox("Resize Minimap Markers", ref enableResizing))
		{
			Services.Config.EnableResizing = enableResizing;
			if (enableResizing)
			{
				_plugin.Enable();
			}
			else
			{
				_plugin.Enable();
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
				Services.Config.MinimapIconScale = float.Sqrt(iconScale / 100f);
			}

			if (changed |= ImGui.Checkbox("Resize Off-map Markers", ref resizeOffMapIcons))
			{
				Services.Config.ResizeOffMapIcons = resizeOffMapIcons;
			}
			ImGuiComponents.HelpMarker("Enables changing the size of far away things like active quests and party members," +
									   "\nrelative to the marker scale.");

			if (resizeOffMapIcons)
			{
				if (changed |= Slider("Off-map Marker Scalar", ref offMapIconScale, 5f, 200f))
				{
					Services.Config.OffMapIconScalar = float.Sqrt(offMapIconScale / 100f) / 0.625f;
				}
				ImGuiComponents.HelpMarker("By default, off-map markers are 39.1% the size of markers that are in minimap radius." +
										   "\nChanging this to 100% will make off-map markers the same size as markers that are in range.");
			}

			if (changed |= ImGui.Checkbox("Override Player Marker Scale", ref overridePlayerMarker))
			{
				Services.Config.OverridePlayerMarker = overridePlayerMarker;
			}

			if (overridePlayerMarker)
			{
				if (changed |= Slider("Player Marker Scale", ref playerMarkerScale, 5f, 400f))
				{
					Services.Config.PlayerMarkerScale = float.Sqrt(playerMarkerScale / 100f);
				}
			}

			if (changed |= ImGui.Checkbox("Override Ally Marker Scale", ref overrideAllyMarkers))
			{
				Services.Config.OverrideAllyMarkers = overrideAllyMarkers;
			}

			if (overrideAllyMarkers)
			{
				if (changed |= Slider("Ally Marker Scale", ref allyMarkerScale, 5f, 400f))
				{
					Services.Config.AllyMarkerScale = float.Sqrt(allyMarkerScale / 100f);
				}
			}
		}

		if (changed)
		{
			Services.Config.Save();
		}
	}

	private static bool Slider(string label, ref float value, float min, float max)
	{
		return ImGui.SliderFloat(label, ref value, min, max,
			value.ToString(@".0\%\%", CultureInfo.CurrentCulture), ImGuiSliderFlags.Logarithmic);
	}
}
