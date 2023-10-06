using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using MinimapMarkerMagnitude.Config;
using System.Globalization;
using System.Numerics;

namespace MinimapMarkerMagnitude.Windows;

internal class ConfigWindow : Window, IDisposable
{
	private readonly Plugin _plugin;
	private int? _currentEditingGroup;

	private string _groupNameInput = string.Empty;

	internal ConfigWindow(Plugin plugin) : base(
		"Minimap Marker Magnitude Settings",
		ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
	{
		_plugin = plugin;
		SizeCondition = ImGuiCond.FirstUseEver;
		Size = new Vector2(660, 360);
		SizeConstraints = new WindowSizeConstraints { MinimumSize = new Vector2(660, 300), MaximumSize = new Vector2(99999, 99999) };
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
	}

	public override void Draw()
	{
		if (_currentEditingGroup == null)
			DrawMainConfig();
		else
			DrawGroupEdit(_currentEditingGroup.Value);
	}

	private void DrawGroupEdit(int editGroup)
	{
		var changed = false;
		var group = Services.Config.IconGroups[editGroup];
		var iconScale = (float)(Math.Pow(group.GroupScale, 2f) * 100f);
		_groupNameInput = group.GroupName;

		ImGui.PushFont(UiBuilder.IconFont);
		if (ImGui.Button(FontAwesomeIcon.ArrowLeft.ToIconString()))
		{
			_currentEditingGroup = null;
			_groupNameInput = string.Empty;
		}

		ImGui.PopFont();
		ImGui.SameLine();

		if (ImGui.InputTextWithHint("##MarkerGroupNameInput", "Edit Group Name", ref _groupNameInput, 100))
		{
			changed = true;
			group.GroupName = _groupNameInput;
		}

		if (Slider("Group Marker Scale", ref iconScale, 5f, 400f))
		{
			group.GroupScale = float.Sqrt(iconScale / 100f);
			changed = true;
		}

		if (ImGui.BeginChild("ScrollableSection", ImGui.GetContentRegionAvail(), false, ImGuiWindowFlags.NoMove))
		{
			var rowItems = 0;
			var itemsPerRow = (int)MathF.Floor(ImGui.GetContentRegionMax().X / (64 + ImGui.GetStyle().ItemSpacing.X));

			foreach (var iconId in Services.SeenIcons)
			{
				var tex = Services.TextureProvider.GetIcon((uint)iconId);
				if (tex is null) continue;
				var selected = group.GroupIconIds.Contains(iconId);

				ImGui.Image(tex.ImGuiHandle,
					new Vector2(64, 64),
					new Vector2(),
					new Vector2(1, 1),
					new Vector4(1, 1, 1, 1),
					selected ? new Vector4(0, 1, 0, 1) : new Vector4(1, 1, 1, 1));

				if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
				{
					if (selected)
						group.GroupIconIds.Remove(iconId);
					else
						group.GroupIconIds.Add(iconId);
					changed = true;
				}

				rowItems++;

				if (rowItems < itemsPerRow)
					ImGui.SameLine();
				else
					rowItems = 0;
			}
		}

		ImGui.EndChild();

		if (changed) Services.Config.Save();
	}

	private void DrawMainConfig()
	{
		const ImGuiTableFlags tableFlags =
			ImGuiTableFlags.RowBg |
			ImGuiTableFlags.Borders |
			ImGuiTableFlags.BordersOuter |
			ImGuiTableFlags.BordersInner |
			ImGuiTableFlags.ScrollY |
			ImGuiTableFlags.NoSavedSettings;

		var changed = false;

		var enableResizing = Services.Config.EnableResizing;
		var iconScale = (float)(Math.Pow(Services.Config.DefaultMinimapIconScale, 2f) * 100f);
		var resizeOffMapIcons = Services.Config.ResizeOffMapIcons;
		var offMapIconScale = (float)(Math.Pow(Services.Config.DefaultOffMapIconScalar * 0.625f, 2f) * 100f);

		if (ImGui.Checkbox("Enable", ref enableResizing))
		{
			Services.Config.EnableResizing = enableResizing;
			changed = true;
			if (enableResizing)
				_plugin.Enable();
			else
				_plugin.Disable();
		}

		ImGui.SameLine();
		ImGui.SetCursorPosX(ImGui.GetContentRegionMax().X * 0.3f);
		ImGui.Text("   Default Marker Scale:");
		ImGui.SameLine();
		ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
		if (Slider("##Default Marker Scale", ref iconScale, 5f, 400f))
		{
			Services.Config.DefaultMinimapIconScale = float.Sqrt(iconScale / 100f);
			changed = true;
		}

		if (ImGui.Checkbox("Resize Off-map Markers", ref resizeOffMapIcons))
		{
			Services.Config.ResizeOffMapIcons = resizeOffMapIcons;
			changed = true;
		}
		ImGui.SameLine();
		ImGui.SetCursorPosX(ImGui.GetContentRegionMax().X * 0.3f);
		ImGui.Text("Off-map Marker Scalar:");
		ImGui.SameLine();
		ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
		if (Slider("##Off-map Marker Scalar", ref offMapIconScale, 5f, 200f))
		{
			Services.Config.DefaultOffMapIconScalar = float.Sqrt(offMapIconScale / 100f) / 0.625f;
			changed = true;
		}

		ImGui.InputTextWithHint("##MarkerGroupNameInput", "New Icon Group Name", ref _groupNameInput, 100);

		ImGui.SameLine();

		ImGui.PushFont(UiBuilder.IconFont);

		if (ImGui.Button(FontAwesomeIcon.Plus.ToIconString()) && !string.IsNullOrWhiteSpace(_groupNameInput))
		{
			Services.Config.IconGroups.Add(new MinimapIconGroup { GroupName = _groupNameInput });
			_groupNameInput = string.Empty;
			changed = true;
		}

		ImGui.PopFont();

		if (ImGui.BeginTable("test", 2, tableFlags))
		{
			ImGui.AlignTextToFramePadding();
			ImGui.TableSetupColumn("Icon Group", ImGuiTableColumnFlags.WidthStretch, 0, 0);
			ImGui.TableSetupColumn("Actions", ImGuiTableColumnFlags.WidthFixed, 60, 1);

			ImGui.TableHeadersRow();

			for (var i = 0; i < Services.Config.IconGroups.Count; i++)
			{
				var group = Services.Config.IconGroups[i];
				ImGui.PushID(i);
				ImGui.TableNextRow();
				ImGui.TableSetColumnIndex(0);
				ImGui.Text(group.GroupName);
				ImGui.TableSetColumnIndex(1);
				ImGui.PushFont(UiBuilder.IconFont);

				if (ImGui.Button(FontAwesomeIcon.Edit.ToIconString())) _currentEditingGroup = i;

				ImGui.SameLine();

				if (ImGui.Button(FontAwesomeIcon.Trash.ToIconString()))
				{
					Services.Config.IconGroups.Remove(group);
					changed = true;
				}

				ImGui.PopFont();
			}

			ImGui.EndTable();
		}

		if (changed) Services.Config.Save();
	}

	private static bool Slider(string label, ref float value, float min, float max)
	{
		return ImGui.SliderFloat(label, ref value, min, max,
			value.ToString(@".0\%\%", CultureInfo.CurrentCulture),
			ImGuiSliderFlags.Logarithmic | ImGuiSliderFlags.AlwaysClamp);
	}
}
