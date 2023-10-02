using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace MinimapMarkerMagnitude;

internal sealed class Services
{
	[PluginService] public static DalamudPluginInterface PluginInterface { get; private set; } = null!;

	[PluginService] public static IClientState ClientState { get; private set; } = null!;

	[PluginService] public static ICommandManager CommandManager { get; private set; } = null!;

	[PluginService] public static IAddonLifecycle AddonLifecycle { get; private set; } = null!;

	[PluginService] public static IGameGui GameGui { get; private set; } = null!;

	[PluginService] public static IPluginLog PluginLog { get; private set; } = null!;

	public static Configuration Config { get; internal set; } = null!;
}
