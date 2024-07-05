using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
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
	private short _iconPreviewSize = (short)(Math.Round(ImGuiHelpers.GlobalScaleSafe, MidpointRounding.AwayFromZero) * 64f);
	private bool _sortIconsById;
	private bool _hideSelectedIcons;

	internal ConfigWindow(Plugin plugin) : base(
		"Minimap Marker Magnitude Settings",
		ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
	{
		_plugin = plugin;
		SizeCondition = ImGuiCond.FirstUseEver;
		Size = new Vector2(660, 360);
		SizeConstraints = new WindowSizeConstraints
		{ MinimumSize = new Vector2(660, 300), MaximumSize = new Vector2(99999, 99999) };
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

		ImGui.Text("Group Name:");
		ImGui.SameLine();
		if (ImGui.InputTextWithHint("##MarkerGroupNameInput", "Edit Group Name", ref _groupNameInput, 100))
		{
			changed = true;
			group.GroupName = _groupNameInput;
		}

		ImGui.Text("Group Marker Scale:");
		ImGui.SameLine();
		if (Slider("##GroupMarkerScale", ref iconScale, 5f, 400f))
		{
			group.GroupScale = float.Sqrt(iconScale / 100f);
			changed = true;
		}

		ImGui.Text("Icon Preview Size:");
		ImGui.SameLine();
		ImGui.SetNextItemWidth(60);
		if (ImGui.BeginCombo("##IconPreviewSelector", _iconPreviewSize + "x"))
		{
			if (ImGui.Selectable(32 + "x")) _iconPreviewSize = 32;
			if (ImGui.Selectable(64 + "x")) _iconPreviewSize = 64;
			if (ImGui.Selectable(128 + "x")) _iconPreviewSize = 128;
			if (ImGui.Selectable(192 + "x")) _iconPreviewSize = 192;
			if (ImGui.Selectable(256 + "x")) _iconPreviewSize = 256;

			ImGui.EndCombo();
		}

		ImGui.SameLine();

		ImGui.Text("Sort Order:");
		ImGui.SameLine();
		ImGui.SetNextItemWidth(120 * ImGuiHelpers.GlobalScale);
		if (ImGui.BeginCombo("##Sorting", _sortIconsById ? "Internal Icon ID" : "Newest First"))
		{
			if (ImGui.Selectable("Newest First")) _sortIconsById = false;

			if (ImGui.Selectable("Internal Icon ID")) _sortIconsById = true;

			ImGui.EndCombo();
		}

		ImGui.SameLine();
		ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 20);
		ImGui.Checkbox("Hide Selected Icons", ref _hideSelectedIcons);
		if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
			ImGui.SetTooltip("This will hide icons that are already part of this group." +
							 "\nIcons from other groups are always hidden, because an icon cannot belong to more than one group.");

		ImGui.SameLine();
		ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 20);

		if (ImGui.IsKeyDown(ImGuiKey.ModCtrl) && ImGui.IsKeyDown(ImGuiKey.ModShift))
		{
			if (ImGui.Button("Delete Icon Group"))
			{
				Services.Config.IconGroups.Remove(group);
				changed = true;
				_currentEditingGroup = null;
			}
		}
		else
		{
			ImGui.BeginDisabled();
			ImGui.Button("Delete Icon Group");
			ImGui.EndDisabled();
			if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
				ImGui.SetTooltip("Hold CTRL+SHIFT to allow deletion");
		}

		if (ImGui.BeginChild("ScrollableSection", ImGui.GetContentRegionAvail(), false, ImGuiWindowFlags.NoMove))
		{
			var rowItems = 0;
			var itemsPerRow = (int)MathF.Floor(ImGui.GetContentRegionMax().X / (_iconPreviewSize + 2 + ImGui.GetStyle().ItemSpacing.X));

			var unassignedIcons = Services.SeenIcons
				.Where(x => !Services.Config.IconGroups
					.SelectMany(y => y.GroupIconIds, (_, iconId) => iconId)
					.Contains(x) || group.GroupIconIds.Contains(x));

			unassignedIcons = _sortIconsById ? unassignedIcons.OrderBy(x => x) : unassignedIcons.Reverse();

			foreach (var iconId in unassignedIcons)
			{
				if (group.GroupIconIds.Contains(iconId) && _hideSelectedIcons)
				{
					continue;
				}

				if (Services.CompiledSizeOverrides.ContainsKey(iconId) && !group.GroupIconIds.Contains(iconId))
					continue;

				var tex = Services.TextureProvider.GetFromGameIcon(new GameIconLookup(iconId)); ;
				// if (tex is null) continue;

				var selected = group.GroupIconIds.Contains(iconId);

				ImGui.Image(tex.GetWrapOrEmpty().ImGuiHandle,
					new Vector2(_iconPreviewSize, _iconPreviewSize),
					new Vector2(),
					new Vector2(1, 1),
					new Vector4(1, 1, 1, 1),
					selected ? new Vector4(0, 1, 0, 1) : new Vector4(1, 1, 1, 1));

				if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled)) ImGui.SetTooltip(iconId.ToString());

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
		ImGui.SetCursorPosX(ImGui.GetContentRegionMax().X * 0.3f + 10);
		ImGui.Text("Default Marker Scale:");
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

		if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
			ImGui.SetTooltip(
				"This affects icons that are anchored to the edge of the minimap.\nFor example, quests in progress that have objectives on the map you're on but are out of minimap range.");

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

		if (ImGui.BeginTable("test", 4, tableFlags))
		{
			ImGui.AlignTextToFramePadding();
			ImGui.TableSetupColumn("Icon Group", ImGuiTableColumnFlags.WidthStretch, 0, 0);
			ImGui.TableSetupColumn("Group Scale", ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Group Scale").X, 1);
			ImGui.TableSetupColumn("Enabled", ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Enabled").X, 2);
			ImGui.TableSetupColumn("Actions", ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Actions").X, 3);

			ImGui.TableHeadersRow();

			for (var i = 0; i < Services.Config.IconGroups.Count; i++)
			{
				var group = Services.Config.IconGroups[i];
				var groupEnabled = group.Enabled;
				ImGui.PushID(i);
				ImGui.TableNextRow();

				ImGui.TableSetColumnIndex(0);
				ImGui.Text(group.GroupName);

				ImGui.TableSetColumnIndex(1);
				ImGui.Text(((float)(Math.Pow(group.GroupScale, 2f) * 100f)).ToString(@"###.#\%\%",
					CultureInfo.CurrentCulture));

				ImGui.TableSetColumnIndex(2);
				if (ImGui.Checkbox("", ref groupEnabled))
				{
					Services.Config.IconGroups[i].Enabled = groupEnabled;
					changed = true;
				}

				ImGui.TableSetColumnIndex(3);
				ImGui.PushFont(UiBuilder.IconFont);
				if (ImGui.Button(FontAwesomeIcon.Edit.ToIconString())) _currentEditingGroup = i;
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
