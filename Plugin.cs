using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Component.GUI;
using MinimapMarkerModifier.Windows;

namespace MinimapMarkerModifier
{
	public sealed class Plugin : IDalamudPlugin
	{
		public string Name => "Minimap Marker Modifier";
		private const string ConfigWindowCommandName = "/mmm";

		private readonly WindowSystem WindowSystem = new("MinimapMarkerModifier");
		private AddonNaviMapUpdateHook AddonHook { get; }
		private GameGui GameGui { get; }
		private DalamudPluginInterface PluginInterface { get; }
		private CommandManager CommandManager { get; }
		private ConfigWindow ConfigWindow { get; }
		private ChatGui ChatGui { get; }
		public Configuration Config { get; }

		public Plugin(
			[RequiredVersion("1.0")] Framework framework,
			[RequiredVersion("1.0")] GameGui gameGui,
			[RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
			[RequiredVersion("1.0")] CommandManager commandManager,
			[RequiredVersion("1.0")] ChatGui chatGui)
		{
			GameGui = gameGui;
			PluginInterface = pluginInterface;
			CommandManager = commandManager;
			ChatGui = chatGui;

			Config = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
			Config.Initialize(PluginInterface);

			ConfigWindow = new ConfigWindow(this);

			WindowSystem.AddWindow(ConfigWindow);

			CommandManager.AddHandler(ConfigWindowCommandName, new CommandInfo(OnConfigWindowCommand)
			{
				HelpMessage = "Opens the Minimap Marker Modifier config window."
			});

			PluginInterface.UiBuilder.Draw += DrawUI;
			PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;

			AddonHook = new AddonNaviMapUpdateHook(this);
		}

		internal unsafe void ResizeIcons(bool reset = false)
		{
			var unitBase = (AtkUnitBase*)GameGui.GetAddonByName("_NaviMap");
			if (unitBase == null) return;

			if (unitBase->UldManager.NodeListCount < 19) return;

			var iconNodeList = unitBase->GetNodeById(18)->GetAsAtkComponentNode();

			for (var i = 0; i < iconNodeList->Component->UldManager.NodeListCount; i++)
			{
				var componentNode = iconNodeList->Component->UldManager.NodeList[i]->GetAsAtkComponentNode();
				if (componentNode == null) continue;
				var collisionNode = componentNode->Component->UldManager.SearchNodeById(7);
				var iconNode = componentNode->Component->GetImageNodeById(3);
				var heightMarkerNode = componentNode->Component->GetImageNodeById(2);
				if (collisionNode is null || iconNode is null || heightMarkerNode is null) continue;
				var imageNode = iconNode->GetAsAtkImageNode();
				if (imageNode is null) continue;

				for (var j = 0; j < imageNode->PartsList->PartCount; j++)
				{
					var iconId = GetTexPathHash(imageNode->PartsList->Parts[j].UldAsset);
					if (iconId == null || iconId == -1) continue;

					if (reset)
					{
						SetScale(collisionNode, iconNode, heightMarkerNode, 1.0f);
						continue;
					}

					SetScale(collisionNode, iconNode, heightMarkerNode, IconMap(iconId.Value));
				}
			}
		}

		private unsafe void SetScale(AtkResNode* collisionNode, AtkResNode* iconNode, AtkResNode* heightMarkerNode, float scale)
		{
			iconNode->SetScale(scale, scale);
			heightMarkerNode->SetScale(scale, scale);
			collisionNode->SetScale(scale, scale);
			collisionNode->SetPositionFloat((1 - scale) * 16, (1 - scale) * 16);
		}

		private unsafe int? GetTexPathHash(AtkUldAsset* uldAsset)
		{
			var res = uldAsset->AtkTexture.Resource;
			if (res is null) return null;
			return res->IconID;
		}

		public void Dispose()
		{
			WindowSystem.RemoveAllWindows();
			ConfigWindow.Dispose();
			CommandManager.RemoveHandler(ConfigWindowCommandName);

			PluginInterface.UiBuilder.Draw -= DrawUI;
			PluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUI;

			AddonHook.Dispose();
		}

		private void OnConfigWindowCommand(string command, string args)
		{
			switch (args)
			{
				case "":
					DrawConfigUI();
					break;

				case "enable" or "on":
					if (Config.EnableResizing)
					{
						ChatGui.Print("Minimap Marker Modifier was already enabled.");
					}
					else
					{
						ChatGui.Print("Minimap Marker Modifier now enabled.");
						Config.EnableResizing = true;
						Config.Save();
					}
					break;

				case "disable" or "off":
					if (Config.EnableResizing)
					{
						ChatGui.Print("Minimap Marker Modifier now disabled.");
						Config.EnableResizing = false;
						Config.Save();
					}
					else
					{
						ChatGui.Print("Minimap Marker Modifier was already disabled.");
					}
					break;

				default:
					ChatGui.PrintError($"Unknown command: '/{command} {args}'");
					break;
			}
		}

		private void DrawUI()
		{
			WindowSystem.Draw();
		}

		public void DrawConfigUI()
		{
			ConfigWindow.IsOpen = true;
		}

		internal float IconMap(int iconId) => iconId switch
		{
			(>= 60409) and (<= 60411) => 1.0f,  // quest search areas
			(>= 60421) and (<= 60424) => 1.0f,  // party members, enemies
			60443 => 1.0f,                      // player marker
			60457 => 1.0f,                      // map transition
			(>= 60495) and (<= 60498) => 1.0f,  // more quest search areas
			_ => Config.MinimapIconScale,
		};
	}
}
