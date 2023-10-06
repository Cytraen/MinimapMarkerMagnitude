using FFXIVClientStructs.FFXIV.Component.GUI;

namespace MinimapMarkerMagnitude;

internal static class ResizeUtil
{
	internal static void CompileIconSizes()
	{
		Services.CompiledSizeOverrides = Services.Config.IconGroups
			.SelectMany(x => x.GroupIconIds, (group, iconId) => new { iconId, group.GroupScale })
			.GroupBy(x => x.iconId)
			.ToDictionary(x => x.First().iconId, x => x.First().GroupScale);
	}

	internal static unsafe void ResizeIcons(bool reset = false)
	{
		var unitBase = (AtkUnitBase*)Services.GameGui.GetAddonByName("_NaviMap");
		if (unitBase is null || unitBase->UldManager.NodeListCount < 19) return;
		var iconNodeList = unitBase->GetNodeById(18)->GetAsAtkComponentNode();

		for (var i = 0; i < iconNodeList->Component->UldManager.NodeListCount; i++)
		{
			var componentNode = iconNodeList->Component->UldManager.NodeList[i]->GetAsAtkComponentNode();
			if (componentNode is null) continue;
			var collisionNode = componentNode->Component->UldManager.SearchNodeById(7);
			var offScreenArrow = componentNode->Component->GetImageNodeById(4);
			var iconNode = componentNode->Component->GetImageNodeById(3);
			var heightMarkerNode = componentNode->Component->GetImageNodeById(2);
			if (collisionNode is null || iconNode is null || heightMarkerNode is null) continue;
			var imageNode = iconNode->GetAsAtkImageNode();
			if (imageNode is null) continue;

			if (imageNode->PartsList->PartCount == 0)
			{
				Services.PluginLog.Warning("AtkImageNode had no parts");
			}

			if (GetIconId(imageNode->PartsList->Parts[0].UldAsset) is not { } iconId || iconId == -1)
			{
				if (imageNode->PartsList->PartCount != 1)
				{
					Services.PluginLog.Warning("More than 1 part and iconId was not -1");
				}
				continue;
			}

			if (!Services.SeenIcons.Contains(iconId))
			{
				Services.SeenIcons.Add(iconId);
				Services.SeenIcons.Save();
			}

			if (reset)
			{
				SetScale(collisionNode, iconNode, heightMarkerNode, 1.0f);
				continue;
			}

			if (Services.Config.ResizeOffMapIcons && offScreenArrow is not null)
			{
				if (!offScreenArrow->IsVisible)
				{
					Services.PluginLog.Warning("Found non-visible arrow");
				}

				SetScale(collisionNode, iconNode, heightMarkerNode, Services.Config.DefaultOffMapIconScalar * IconMap(iconId));
			}
			else
			{
				SetScale(collisionNode, iconNode, heightMarkerNode, IconMap(iconId));
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

	private static float IconMap(int iconId) => iconId switch
	{
		>= 60409 and <= 60411 => 1.0f,  // quest search areas
		>= 60421 and <= 60424 => 1.0f,  // party members, enemies
		60443 => 1.0f,                  // player marker
		60457 => 1.0f,                  // map transition
		60469 or 60470 => 1.0f,         // party member and alliance member?
		>= 60495 and <= 60498 => 1.0f,  // more quest search areas
		60961 => 1.0f,                  // pet marker
		_ => Services.CompiledSizeOverrides.TryGetValue(iconId, out var size) ? size : Services.Config.DefaultMinimapIconScale,
	};
}
