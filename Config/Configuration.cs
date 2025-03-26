using System.Text.Json;
using Dalamud.Configuration;

namespace MinimapMarkerMagnitude.Config;

public class Configuration : IPluginConfiguration
{
	private const string FileName = "Config.json";

	private readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

	public bool EnableResizing { get; set; } = true;

	public float DefaultMinimapIconScale { get; set; } = 1f;

	public bool ResizeOffMapIcons { get; set; }

	public float DefaultOffMapIconScalar { get; set; } = 1f;

	public List<MinimapIconGroup> IconGroups { get; set; } = [];

	public int Version { get; set; }

	public static void Load()
	{
		var loc = Path.Combine(Services.PluginInterface.GetPluginConfigDirectory(), FileName);
		if (File.Exists(loc))
		{
			Services.Config = JsonSerializer.Deserialize<Configuration>(File.ReadAllText(loc))!;
		}
		else
		{
			Services.Config = new Configuration { IconGroups = DefaultIconGroups() };
			Services.Config.Save();
		}
	}

	public void Save()
	{
		File.WriteAllText(
			Path.Combine(Services.PluginInterface.GetPluginConfigDirectory(), FileName),
			JsonSerializer.Serialize(this, _serializerOptions)
		);
		ResizeUtil.CompileIconSizes();
	}

	private static List<MinimapIconGroup> DefaultIconGroups()
	{
		return
		[
			new MinimapIconGroup
			{
				GroupName = "Guilds",
				Enabled = false,
				GroupIconIds =
				[
					60318,
					60319,
					60320,
					60321,
					60322,
					60326,
					60330,
					60331,
					60333,
					60334,
					60335,
					60337,
					60342,
					60344,
					60345,
					60346,
					60347,
					60348,
					60351,
					60362,
					60363,
					60364,
				],
			},
			new MinimapIconGroup
			{
				GroupName = "FATEs",
				Enabled = false,
				GroupIconIds =
				[
					60458,
					60501,
					60502,
					60503,
					60504,
					60505,
					60506,
					60508,
					60511,
					60512,
					60513,
					60514,
					60515,
				],
			},
			new MinimapIconGroup
			{
				GroupName = "Main Scenario Quests",
				Enabled = false,
				GroupIconIds =
				[
					70961,
					70962,
					70963,
					70964,
					71001,
					71002,
					71003,
					71004,
					71005,
					71006,
					71011,
					71012,
					71013,
					71014,
					71015,
					71016,
				],
			},
			new MinimapIconGroup
			{
				GroupName = "Feature/Job Quests",
				Enabled = false,
				GroupIconIds =
				[
					70971,
					70972,
					70973,
					70974,
					70975,
					70976,
					71141,
					71142,
					71143,
					71144,
					71145,
					71146,
					71151,
					71152,
					71153,
					71154,
					71155,
					71156,
				],
			},
			new MinimapIconGroup
			{
				GroupName = "Side Quests",
				Enabled = false,
				GroupIconIds =
				[
					70965,
					70966,
					70967,
					70968,
					70969,
					70970,
					71021,
					71022,
					71023,
					71024,
					71025,
					71026,
					71031,
					71032,
					71033,
					71034,
					71035,
					71036,
				],
			},
			new MinimapIconGroup
			{
				GroupName = "Levequests",
				Enabled = false,
				GroupIconIds =
				[
					71041,
					71042,
					71043,
					71044,
					71045,
					71046,
					71051,
					71052,
					71053,
					71054,
					71055,
					71056,
				],
			},
			new MinimapIconGroup
			{
				GroupName = "Guildhests",
				Enabled = false,
				GroupIconIds =
				[
					71081,
					71082,
					71083,
					71084,
					71085,
					71086,
					71091,
					71092,
					71093,
					71094,
					71095,
					71096,
				],
			},
			new MinimapIconGroup
			{
				GroupName = "Hall of the Novice Quests",
				Enabled = false,
				GroupIconIds =
				[
					71121,
					71122,
					71123,
					71124,
					71125,
					71126,
					71131,
					71132,
					71133,
					71134,
					71135,
					71136,
				],
			},
			new MinimapIconGroup
			{
				// TODO better name
				GroupName = "Lore 'Quests'",
				Enabled = false,
				GroupIconIds =
				[
					71061,
					71062,
					71063,
					71064,
					71065,
					71066,
					71071,
					71072,
					71073,
					71074,
					71075,
					71076,
				],
			},
			new MinimapIconGroup
			{
				GroupName = "Triple Triad & GATEs",
				Enabled = false,
				GroupIconIds = [71101, 71102, 71111, 71112, 71113],
			},
		];
	}
}

public class MinimapIconGroup
{
	public string GroupName { get; set; } = null!;

	public bool Enabled { get; set; } = true;

	public HashSet<uint> GroupIconIds { get; set; } = [];

	public float GroupScale { get; set; } = 1.0f;
}
