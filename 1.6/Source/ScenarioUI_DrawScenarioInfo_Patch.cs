using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace ProgressionScenarios
{
	[HotSwappable]
	[HarmonyPatch(typeof(ScenarioUI), "DrawScenarioInfo")]
	public static class ScenarioUI_DrawScenarioInfo_Patch
	{
		private const float IconSize = 30f;
		private const float IconGap = 8f;

		public static bool Prefix(Rect rect, Scenario scen, ref Vector2 infoScrollPosition)
		{
			if (scen == null) return true;
			var icon = scen.GetIcon();
			if (icon == null) return true;

			Widgets.DrawMenuSection(rect);
			rect = rect.GetInnerRect();
			var fullInformationText = scen.GetFullInformationText();
			var width = rect.width - 16f;
			var viewRect = new Rect(0f, 0f, width, 30f + Text.CalcHeight(fullInformationText, width) + 100f);
			Widgets.BeginScrollView(rect, ref infoScrollPosition, viewRect);
			GUI.DrawTexture(new Rect(0f, 0f, IconSize, IconSize), icon);
			Text.Font = GameFont.Medium;
			Widgets.Label(new Rect(IconSize + IconGap, 0f, viewRect.width - IconSize - IconGap, 30f), scen.name);
			Text.Font = GameFont.Small;
			Widgets.Label(new Rect(0f, 30f, viewRect.width, viewRect.height - 30f), fullInformationText);
			Widgets.EndScrollView();
			return false;
		}
	}
}
