using Dalamud.Game;
using Dalamud.Game.ClientState;
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
		private Framework Framework { get; }
		private GameGui GameGui { get; }
		private DalamudPluginInterface PluginInterface { get; }
		private CommandManager CommandManager { get; }
		private ClientState ClientState { get; }
		private ConfigWindow ConfigWindow { get; }
		private ChatGui ChatGui { get; }
		public Configuration Config { get; }

		public Plugin(
			[RequiredVersion("1.0")] Framework framework,
			[RequiredVersion("1.0")] GameGui gameGui,
			[RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
			[RequiredVersion("1.0")] CommandManager commandManager,
			[RequiredVersion("1.0")] ClientState clientState,
			[RequiredVersion("1.0")] ChatGui chatGui)
		{
			Framework = framework;
			GameGui = gameGui;
			PluginInterface = pluginInterface;
			CommandManager = commandManager;
			ClientState = clientState;
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

			if (Config.EnableResizing)
			{
				EnablePlugin();
			}
		}

		internal void EnablePlugin()
		{
			Framework.Update -= MinimapTick;
			Framework.Update += MinimapTick;
		}

		internal void DisablePlugin()
		{
			Framework.Update -= MinimapTick;
			ResizeIcons(true);
		}

		private void MinimapTick(Framework framework)
		{
			ResizeIcons();
		}

		private unsafe void SetScale(AtkResNode* node, float scale)
		{
			node->SetScale(scale, scale);
		}

		private unsafe void ResizeIcons(bool reset = false)
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

				for (var j = 0; j < imageNode->PartsList->PartCount; j++)
				{
					var iconId = GetTexPathHash(imageNode->PartsList->Parts[j].UldAsset);
					if (iconId == null || iconId == -1) continue;
					if (reset) { SetScale(iconNode, 1.0f); continue; }
					switch (iconId)
					{
						case 60443: // player marker
						case 60457: // map transition
						case 60495: // quest radius marker
						case 60496: // quest radius marker
						case 60497: // quest radius marker
						case 60498: // quest radius marker
							SetScale(collisionNode, 1.0f);
							collisionNode->SetPositionFloat(0, 0);
							SetScale(iconNode, 1.0f);
							continue;

						default:
							SetScale(collisionNode, Config.MinimapIconScale);
							collisionNode->SetPositionFloat((1 - Config.MinimapIconScale) * 16, (1 - Config.MinimapIconScale) * 16);
							SetScale(iconNode, Config.MinimapIconScale);
							SetScale(heightMarkerNode, Config.MinimapIconScale);
							break;
					}
				}
			}
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

			DisablePlugin();
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
						EnablePlugin();
					}
					break;

				case "disable" or "off":
					if (Config.EnableResizing)
					{
						ChatGui.Print("Minimap Marker Modifier now disabled.");
						Config.EnableResizing = false;
						Config.Save();
						DisablePlugin();
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
	}
}
