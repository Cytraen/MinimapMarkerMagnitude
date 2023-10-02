using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using MinimapMarkerMagnitude.Windows;

namespace MinimapMarkerMagnitude;

internal sealed class Plugin : IDalamudPlugin
{
	private const string ConfigWindowCommandName = "/mmm";
	private readonly ConfigWindow _configWindow;
	private readonly WindowSystem _windowSystem;

	public Plugin(DalamudPluginInterface pluginInterface)
	{
		pluginInterface.Create<Services>();

		_windowSystem = new WindowSystem("MinimapMarkerMagnitude");

		Services.Config = Services.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

		_configWindow = new ConfigWindow(this);

		_windowSystem.AddWindow(_configWindow);

		Services.CommandManager.AddHandler(ConfigWindowCommandName, new CommandInfo(OnConfigWindowCommand)
		{
			HelpMessage = "Opens the Minimap Marker Magnitude config window."
		});

		Services.PluginInterface.UiBuilder.Draw += DrawUi;
		Services.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUi;

		Services.ClientState.Login += Enable;
		Services.ClientState.Logout += Disable;

		if (Services.ClientState.IsLoggedIn)
		{
			Enable();
		}
	}

	internal void Enable()
	{
		if (Services.Config.EnableResizing)
		{
			Services.AddonLifecycle.RegisterListener(AddonEvent.PreUpdate, "_NaviMap", NaviMapPreUpdateListener);
		}
	}

	internal void Disable()
	{
		Services.AddonLifecycle.UnregisterListener(NaviMapPreUpdateListener);
		ResizeUtil.ResizeIcons();
	}

	private void NaviMapPreUpdateListener(AddonEvent addonEvent, AddonArgs addonArgs)
	{
		try
		{
			ResizeUtil.ResizeIcons();
		}
		catch (Exception ex)
		{
			Services.PluginLog.Error(ex, "An error occurred when handling a AddonNaviMap_Update.");
			Disable();
		}
	}

	public void Dispose()
	{
		_windowSystem.RemoveAllWindows();
		_configWindow.Dispose();
		Services.CommandManager.RemoveHandler(ConfigWindowCommandName);

		Services.PluginInterface.UiBuilder.Draw -= DrawUi;
		Services.PluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUi;

		Services.ClientState.Login -= Enable;
		Services.ClientState.Logout -= Disable;
	}

	private void OnConfigWindowCommand(string command, string args)
	{
		DrawConfigUi();
	}

	private void DrawUi()
	{
		_windowSystem.Draw();
	}

	private void DrawConfigUi()
	{
		_configWindow.IsOpen = true;
	}
}
