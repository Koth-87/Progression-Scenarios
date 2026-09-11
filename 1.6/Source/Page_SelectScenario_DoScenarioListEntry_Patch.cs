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
			var inner = rect.ContractedBy(4f);
			Utils.DrawRecommendedLabel(new Rect(inner.x, inner.y, inner.width, 22f), Page_SelectScenario_PreOpen_Patch.openedAt, TextAnchor.UpperRight);
			Text.Font = font;
		}
	}
}
