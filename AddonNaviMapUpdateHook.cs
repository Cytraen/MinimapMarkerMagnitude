using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Utility.Signatures;

namespace MinimapMarkerModifier;

public unsafe class AddonNaviMapUpdateHook : IDisposable
{
	private delegate void AddonNaviMapUpdateDelegate(long a1, long a2, long a3, long a4);

	[Signature("40 57 48 83 EC 20 48 8B F9 E8 ?? ?? ?? ?? F6 87 ?? ?? ?? ?? ?? 74 70", DetourName = nameof(DetourAddonNaviMapUpdate))]
	private Hook<AddonNaviMapUpdateDelegate>? _macroUpdateHook;

	private Plugin Plugin { get; }

	public AddonNaviMapUpdateHook(Plugin plugin)
	{
		SignatureHelper.Initialise(this);
		_macroUpdateHook?.Enable();
		Plugin = plugin;
	}

	private void DetourAddonNaviMapUpdate(long a1, long a2, long a3, long a4)
	{
		try
		{
			if (Plugin.Config.EnableResizing)
			{
				Plugin.ResizeIcons();
			}
		}
		catch (Exception ex)
		{
			PluginLog.Error(ex, "An error occured when handling a AddonNaviMap_Update.");
		}

		_macroUpdateHook!.Original(a1, a2, a3, a4);
	}

	public void Dispose()
	{
		_macroUpdateHook?.Dispose();
		GC.SuppressFinalize(this);
	}
}
