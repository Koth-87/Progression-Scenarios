using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace ProgressionScenarios
{
	[HotSwappable]
	[HarmonyPatch(typeof(Page_SelectScenario), "DoScenarioListEntry")]
	public static class Page_SelectScenario_DoScenarioListEntry_Patch
	{
		public static void Prefix(ref Rect rect, Scenario scen)
		{
			if (Utils.ScenarioSearchModActive) rect.y += Utils.ScenarioSearchListOffset;
			var icon = scen.GetIcon();
			if (icon == null) return;
			rect.x -= 4;
			var iconRect = new Rect(rect.x, rect.y + (rect.height - 40f) / 2f, 40f, 40f);
			GUI.DrawTexture(iconRect, icon);
			rect.xMin += 48f;
			rect.width += 4f;
		}

		public static void Postfix(Rect rect, Scenario scen)
		{
			if (!scen.IsRecommended()) return;
			var font = Text.Font;
			Utils.DrawRecommendedTag(rect, Page_SelectScenario_PreOpen_Patch.openedAt, false);
			Text.Font = font;
		}
	}

	[HarmonyPatch(typeof(Page_SelectScenario), "ListScenariosOnListing")]
	public static class Page_SelectScenario_ListScenariosOnListing_Patch
	{
		public static void Prefix(Listing_Standard listing, IEnumerable<Scenario> scenarios)
		{
			if (listing.CurHeight > 0f) return;
			var first = scenarios.FirstOrDefault(s => s.showInUI);
			if (first != null && first.IsRecommended()) listing.Gap(6f);
		}
	}
}
