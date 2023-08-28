using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using MinimapMarkerMagnitude.Windows;

namespace MinimapMarkerMagnitude;

internal sealed class Plugin : IDalamudPlugin
{
	public string Name => "Minimap Marker Magnitude";
	private const string ConfigWindowCommandName = "/mmm";

	private readonly AddonNaviMapUpdateHook _addonHook;
	private readonly WindowSystem _windowSystem;
	private readonly DalamudPluginInterface _pluginInterface;
	private readonly ClientState _clientState;
	private readonly CommandManager _commandManager;
	private readonly ConfigWindow _configWindow;

	public Plugin(
		[RequiredVersion("1.0")] GameGui gameGui,
		[RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
		[RequiredVersion("1.0")] ClientState clientState,
		[RequiredVersion("1.0")] CommandManager commandManager)
	{
		_windowSystem = new WindowSystem("MinimapMarkerMagnitude");
		_pluginInterface = pluginInterface;
		_clientState = clientState;
		_commandManager = commandManager;

		var config = _pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
		config.Initialize(_pluginInterface);

		_addonHook = new AddonNaviMapUpdateHook(config, gameGui);

		_configWindow = new ConfigWindow(config, _addonHook);

		_windowSystem.AddWindow(_configWindow);

		_commandManager.AddHandler(ConfigWindowCommandName, new CommandInfo(OnConfigWindowCommand)
		{
			HelpMessage = "Opens the Minimap Marker Magnitude config window."
		});

		_pluginInterface.UiBuilder.Draw += DrawUi;
		_pluginInterface.UiBuilder.OpenConfigUi += DrawConfigUi;

		_clientState.Login += Login;
		_clientState.Logout += Logout;

		if (_clientState.IsLoggedIn)
		{
			_addonHook.Enable();
		}
	}

	private void Login(object? sender, EventArgs e)
	{
		_addonHook.Enable();
	}

	private void Logout(object? sender, EventArgs e)
	{
		_addonHook.Disable();
	}

	public void Dispose()
	{
		_windowSystem.RemoveAllWindows();
		_configWindow.Dispose();
		_commandManager.RemoveHandler(ConfigWindowCommandName);

		_pluginInterface.UiBuilder.Draw -= DrawUi;
		_pluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUi;

		_clientState.Login -= Login;
		_clientState.Logout -= Logout;

		_addonHook.Dispose();
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
