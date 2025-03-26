using System.Text.Json;

namespace MinimapMarkerMagnitude.Config;

public class SeenIconSet : HashSet<uint>
{
	private const string FileName = "SeenIcons.json";

	public static void Load()
	{
		var loc = Path.Combine(Services.PluginInterface.GetPluginConfigDirectory(), FileName);
		if (File.Exists(loc))
		{
			var iconSet = JsonSerializer.Deserialize<SeenIconSet>(File.ReadAllText(loc))!;
			iconSet.RemoveWhere(ResizeUtil.IsBannedIcon);
			Services.SeenIcons = iconSet;
		}
		else
		{
			Services.SeenIcons = [];
		}
		Services.SeenIcons.InitFromConfig();
		Services.SeenIcons.Save();
	}

	internal void InitFromConfig()
	{
		foreach (
			var icon in Services.Config.IconGroups.SelectMany(
				x => x.GroupIconIds,
				(_, iconId) => iconId
			)
		)
		{
			Add(icon);
		}
	}

	public void Save()
	{
		File.WriteAllText(
			Path.Combine(Services.PluginInterface.GetPluginConfigDirectory(), FileName),
			JsonSerializer.Serialize(this)
		);
	}
}
