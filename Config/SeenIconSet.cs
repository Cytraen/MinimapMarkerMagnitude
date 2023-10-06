using System.Text.Json;

namespace MinimapMarkerMagnitude.Config;

public class SeenIconSet : HashSet<int>
{
	private const string FileName = "SeenIcons.json";

	public static void Load()
	{
		var loc = Path.Combine(Services.PluginInterface.GetPluginConfigDirectory(), FileName);
		if (File.Exists(loc))
		{
			Services.SeenIcons = JsonSerializer.Deserialize<SeenIconSet>(File.ReadAllText(loc))!;
		}
		else
		{
			Services.SeenIcons = new SeenIconSet();
			Services.SeenIcons.Save();
		}
	}

	public void Save()
	{
		File.WriteAllText(Path.Combine(Services.PluginInterface.GetPluginConfigDirectory(), FileName),
			JsonSerializer.Serialize(this));
	}
}
