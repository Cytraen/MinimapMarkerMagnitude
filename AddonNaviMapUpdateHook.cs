using Dalamud.Game.Gui;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Utility.Signatures;

namespace MinimapMarkerMagnitude;

internal class AddonNaviMapUpdateHook : IDisposable
{
	private delegate void AddonNaviMapUpdateDelegate(long a1, long a2, long a3, long a4);

	[Signature("40 57 48 83 EC 20 48 8B F9 E8 ?? ?? ?? ?? F6 87 ?? ?? ?? ?? ?? 74 70", DetourName = nameof(DetourAddonNaviMapUpdate))]
	private readonly Hook<AddonNaviMapUpdateDelegate>? _macroUpdateHook;

	private readonly Configuration _config;
	private readonly ResizeUtil _resizeUtil;

	internal AddonNaviMapUpdateHook(Configuration config, GameGui gameGui)
	{
		SignatureHelper.Initialise(this);

		if (_macroUpdateHook is null)
		{
			throw new NullReferenceException("'AddonNaviMap_Update' signature is incorrect.");
		}

		_config = config;
		_resizeUtil = new ResizeUtil(gameGui, _config);
	}

	internal void Enable()
	{
		if (_config.EnableResizing)
		{
			_macroUpdateHook?.Enable();
		}
	}

	internal void Disable()
	{
		_macroUpdateHook?.Disable();
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
		_macroUpdateHook!.Original(a1, a2, a3, a4);
	}

	public void Dispose()
	{
		Disable();
		_macroUpdateHook?.Dispose();
		GC.SuppressFinalize(this);
	}
}
