using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using MinimapMarkerMagnitude.Config;
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
		Configuration.Load();
		SeenIconSet.Load();

		ResizeUtil.CompileIconSizes();

		_windowSystem = new WindowSystem("MinimapMarkerMagnitude");
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

		if (Services.ClientState.IsLoggedIn) Enable();
	}

	public void Dispose()
	{
		Disable();

		_windowSystem.RemoveAllWindows();
		_configWindow.Dispose();
		Services.CommandManager.RemoveHandler(ConfigWindowCommandName);

		Services.PluginInterface.UiBuilder.Draw -= DrawUi;
		Services.PluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUi;

		Services.ClientState.Login -= Enable;
		Services.ClientState.Logout -= Disable;
	}

	internal void Enable()
	{
		if (Services.Config.EnableResizing)
			Services.AddonLifecycle.RegisterListener(AddonEvent.PreUpdate, "_NaviMap", NaviMapPreUpdateListener);
	}

	internal void Disable()
	{
		Services.AddonLifecycle.UnregisterListener(NaviMapPreUpdateListener);
		ResizeUtil.ResizeIcons(true);
	}

	private void NaviMapPreUpdateListener(AddonEvent addonEvent, AddonArgs addonArgs)
	{
		try
		{
			ResizeUtil.ResizeIcons();
		}
		catch (Exception ex)
		{
			Services.PluginLog.Error(ex, "An error occurred when handling _NaviMap PostUpdate.");
			Disable();
		}
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
