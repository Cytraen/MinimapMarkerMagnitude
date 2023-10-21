using FFXIVClientStructs.FFXIV.Component.GUI;

namespace MinimapMarkerMagnitude;

internal static class ResizeUtil
{
	internal static void CompileIconSizes()
	{
		Services.CompiledSizeOverrides = Services.Config.IconGroups
			.Where(x => x.Enabled)
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

			if (GetIconId(imageNode->PartsList->Parts[0].UldAsset) is not { } iconId || iconId == -1)
			{
				continue;
			}

			if (reset || IsBannedIcon(iconId))
			{
				SetScale(collisionNode, iconNode, heightMarkerNode, 1.0f);
				continue;
			}

			if (!componentNode->AtkResNode.IsVisible)
			{
				continue;
			}

			if (!Services.SeenIcons.Contains(iconId))
			{
				if (iconId > 200_000)
				{
					Services.PluginLog.Warning($"Found iconId {iconId} over 200,000");
				}
				Services.SeenIcons.Add(iconId);
				Services.SeenIcons.Save();
			}

			if (Services.Config.ResizeOffMapIcons && offScreenArrow is not null)
			{
				SetScale(collisionNode, iconNode, heightMarkerNode, Services.Config.DefaultOffMapIconScalar * GetIconScale(iconId));
			}
			else
			{
				SetScale(collisionNode, iconNode, heightMarkerNode, GetIconScale(iconId));
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

	internal static bool IsBannedIcon(int iconId) => iconId switch
	{
		>= 60409 and <= 60411 => true,  // quest search areas
		60457 => true,                  // map transition
		>= 60495 and <= 60498 => true,  // more quest search areas
		60566 => true,                  // another search area?
		_ => false,
	};

	private static float GetIconScale(int iconId)
	{
		if (IsBannedIcon(iconId)) return 1.0f;
		return Services.CompiledSizeOverrides.TryGetValue(iconId, out var size)
			? size
			: Services.Config.DefaultMinimapIconScale;
	}
}
