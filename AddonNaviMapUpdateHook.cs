using Dalamud.Game;
using Dalamud.Game.Gui;
using Dalamud.Hooking;
using Dalamud.Logging;

namespace MinimapMarkerMagnitude;

internal class AddonNaviMapUpdateHook : IDisposable
{
	private const string AddonNaviMapUpdateSig = "40 57 48 83 EC 20 48 8B F9 E8 ?? ?? ?? ?? F6 87 ?? ?? ?? ?? ?? 74 70";

	private delegate void AddonNaviMapUpdateDelegate(long a1, long a2, long a3, long a4);

	private readonly Hook<AddonNaviMapUpdateDelegate> _minimapUpdateHook;
	private readonly Configuration _config;
	private readonly ResizeUtil _resizeUtil;

	internal AddonNaviMapUpdateHook(ISigScanner sigScanner, Configuration config, GameGui gameGui)
	{
		if (!sigScanner.TryScanText(AddonNaviMapUpdateSig, out var minimapUpdatePtr))
		{
			throw new NullReferenceException("'AddonNaviMap_Update' signature is incorrect.");
		}

		_config = config;
		_resizeUtil = new ResizeUtil(gameGui, _config);
		_minimapUpdateHook = Hook<AddonNaviMapUpdateDelegate>.FromAddress(minimapUpdatePtr, DetourAddonNaviMapUpdate);
	}

	internal void Enable()
	{
		if (_config.EnableResizing)
		{
			_minimapUpdateHook.Enable();
		}
	}

	internal void Disable()
	{
		_minimapUpdateHook.Disable();
		_resizeUtil.ResizeIcons(true);
	}

	private void DetourAddonNaviMapUpdate(long a1, long a2, long a3, long a4)
	{
		try
		{
			_resizeUtil.ResizeIcons();
		}
		catch (Exception ex)
		{
			PluginLog.Error(ex, "An error occurred when handling a AddonNaviMap_Update.");
		}
		_minimapUpdateHook.Original(a1, a2, a3, a4);
	}

	public void Dispose()
	{
		Disable();
		_minimapUpdateHook.Dispose();
		GC.SuppressFinalize(this);
	}
}
