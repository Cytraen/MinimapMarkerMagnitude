using Dalamud.Game.Gui;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace MinimapMarkerMagnitude;

internal class ResizeUtil
{
	private readonly GameGui _gameGui;

	private readonly Configuration _config;

	internal ResizeUtil(GameGui gameGui, Configuration config)
	{
		_gameGui = gameGui;
		_config = config;
	}

	internal unsafe void ResizeIcons(bool reset = false)
	{
		var unitBase = (AtkUnitBase*)_gameGui.GetAddonByName("_NaviMap");
		if (unitBase is null || unitBase->UldManager.NodeListCount < 19) return;
		var iconNodeList = unitBase->GetNodeById(18)->GetAsAtkComponentNode();

		for (var i = 0; i < iconNodeList->Component->UldManager.NodeListCount; i++)
		{
			var componentNode = iconNodeList->Component->UldManager.NodeList[i]->GetAsAtkComponentNode();
			if (componentNode is null) continue;
			var collisionNode = componentNode->Component->UldManager.SearchNodeById(7);
			var iconNode = componentNode->Component->GetImageNodeById(3);
			var heightMarkerNode = componentNode->Component->GetImageNodeById(2);
			if (collisionNode is null || iconNode is null || heightMarkerNode is null) continue;
			var imageNode = iconNode->GetAsAtkImageNode();
			if (imageNode is null) continue;

			for (var j = 0; j < imageNode->PartsList->PartCount; j++)
			{
				var iconId = GetIconId(imageNode->PartsList->Parts[j].UldAsset);
				if (iconId is null or -1) continue;

				if (reset)
				{
					SetScale(collisionNode, iconNode, heightMarkerNode, 1.0f);
					continue;
				}

				SetScale(collisionNode, iconNode, heightMarkerNode, IconMap(iconId.Value));
			}
		}
	}

	private static unsafe void SetScale(AtkResNode* collisionNode, AtkResNode* iconNode, AtkResNode* heightMarkerNode, float scale)
	{
		iconNode->SetScale(scale, scale);
		heightMarkerNode->SetScale(scale, scale);
		collisionNode->SetScale(scale, scale);
		collisionNode->SetPositionFloat((1 - scale) * 16, (1 - scale) * 16);
	}

	private static unsafe int? GetIconId(AtkUldAsset* uldAsset)
	{
		var res = uldAsset->AtkTexture.Resource;
		if (res is null) return null;
		return res->IconID;
	}

	private float IconMap(int iconId) => iconId switch
	{
		>= 60409 and <= 60411 => 1.0f,  // quest search areas
		>= 60421 and <= 60424 => 1.0f,  // party members, enemies
		60443 => 1.0f,                  // player marker
		60457 => 1.0f,                  // map transition
		>= 60495 and <= 60498 => 1.0f,  // more quest search areas
		_ => _config.MinimapIconScale,
	};
}
