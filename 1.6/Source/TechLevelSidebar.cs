using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ProgressionScenarios
{
	[HotSwappable]
	public static class TechLevelSidebar
	{
		public const float Width = 144f;
		public const float Gap = 14f;

		private const float IconStripSize = 14f;
		private const int IconStripMax = 4;

		private static readonly Color AccentColor = new Color(0.72f, 0.66f, 0.52f);
		private static readonly Color SelectedText = new Color(1f, 0.92f, 0.72f);

		private static readonly TechLevel[][] Groups =
		{
			new[] { TechLevel.Animal },
			new[] { TechLevel.Neolithic, TechLevel.Medieval, TechLevel.Industrial },
			new[] { TechLevel.Spacer, TechLevel.Ultra, TechLevel.Archotech }
		};

		public static bool Active => ScenarioLister_ScenariosInCategory_Patch.selectedLevel != TechLevel.Undefined;

		public static void Draw(Rect rect, Page_SelectScenario page)
		{
			Widgets.DrawBoxSolid(rect, new Color(1f, 1f, 1f, 0.025f));
			GUI.color = new Color(1f, 1f, 1f, 0.12f);
			Widgets.DrawBox(rect);
			GUI.color = Color.white;

			var y = rect.y + 8f;
			Text.Font = GameFont.Tiny;
			Text.Anchor = TextAnchor.MiddleLeft;
			GUI.color = ColoredText.SubtleGrayColor;
			Widgets.Label(new Rect(rect.x + 10f, y, rect.width - 20f, 20f), "PS_TechLevelSidebarHeader".Translate());
			GUI.color = Color.white;
			y += 24f;

			for (var g = 0; g < Groups.Length; g++)
			{
				if (g > 0)
				{
					y += 4f;
					GUI.color = new Color(1f, 1f, 1f, 0.15f);
					Widgets.DrawLineHorizontal(rect.x + 14f, y, rect.width - 28f);
					GUI.color = Color.white;
					y += 8f;
				}
				foreach (var techLevel in Groups[g])
				{
					DrawEntry(new Rect(rect.x + 6f, y, rect.width - 12f, 72f), page, techLevel);
					y += 76f;
				}
			}

			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.color = Color.white;
		}

		private static void DrawEntry(Rect rect, Page_SelectScenario page, TechLevel techLevel)
		{
			var scenarios = techLevel.Scenarios();
			var empty = scenarios.Count == 0;
			var isSelected = techLevel == ScenarioLister_ScenariosInCategory_Patch.selectedLevel;

			Widgets.DrawOptionBackground(rect, isSelected);
			if (isSelected)
			{
				Widgets.DrawBoxSolid(new Rect(rect.x, rect.y, 3f, rect.height), AccentColor);
			}

			var fade = empty ? 0.4f : 1f;
			var iconRect = new Rect(rect.x + 7f, rect.center.y - 16f, 32f, 32f);
			GUI.color = new Color(1f, 1f, 1f, fade);
			var tex = ContentFinder<Texture2D>.Get("UI/Icons/" + techLevel, false);
			if (tex != null)
			{
				GUI.DrawTexture(iconRect, tex, ScaleMode.ScaleToFit);
			}

			var textX = iconRect.xMax + 7f;
			var textWidth = rect.xMax - textX - 4f;
			var top = rect.y + 8f;

			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.color = isSelected ? SelectedText : new Color(1f, 1f, 1f, fade);
			Widgets.Label(new Rect(textX, top, textWidth, 24f), techLevel.GetTechLevelName());

			Text.Font = GameFont.Tiny;
			var sub = ColoredText.SubtleGrayColor;
			GUI.color = new Color(sub.r, sub.g, sub.b, fade);
			var count = scenarios.Count == 1 ? "PS_ScenarioCountOne".Translate() : "PS_ScenarioCount".Translate(scenarios.Count);
			Widgets.Label(new Rect(textX, top + 22f, textWidth, 18f), count);
			GUI.color = Color.white;

			if (!empty)
			{
				Utils.DrawScenarioIconStrip(new Rect(textX, top + 40f, textWidth, IconStripSize), scenarios, IconStripSize, IconStripMax);
			}

			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperLeft;

			if (!empty)
			{
				Utils.ScenarioListTip(rect, techLevel, ("PS_TechLevelDesc_" + techLevel).Translate());
			}

			if (!isSelected && Widgets.ButtonInvisible(rect))
			{
				if (empty)
				{
					Messages.Message("PS_NoScenariosForTechLevel".Translate(), MessageTypeDefOf.RejectInput, historical: false);
				}
				else
				{
					SoundDefOf.Tick_High.PlayOneShotOnCamera();
					Select(page, techLevel);
				}
			}
		}

		private static void Select(Page_SelectScenario page, TechLevel techLevel)
		{
			ScenarioLister_ScenariosInCategory_Patch.selectedLevel = techLevel;
			page.curScen = ScenarioLister.ScenariosInCategory(ScenarioCategory.FromDef).FirstOrDefault()
			               ?? ScenarioLister.ScenariosInCategory(ScenarioCategory.CustomLocal).FirstOrDefault()
			               ?? ScenarioLister.ScenariosInCategory(ScenarioCategory.SteamWorkshop).FirstOrDefault();
			page.scenariosScrollPosition = Vector2.zero;
			page.infoScrollPosition = Vector2.zero;
			Page_SelectScenario_PreOpen_Patch.openedAt = Time.realtimeSinceStartup;
		}

		public static Rect AdjustMainRect(Rect mainRect, Page_SelectScenario page)
		{
			if (!Active)
			{
				return mainRect;
			}
			return SidebarDrawer.DrawAndShift(mainRect, Width, Gap, "PS_HideTechLevelSidebar".Translate(), "PS_ShowTechLevelSidebar".Translate(), rect => Draw(rect, page));
		}
	}

	[HarmonyPatch(typeof(Page_SelectScenario), nameof(Page_SelectScenario.DoWindowContents))]
	public static class Page_SelectScenario_DoWindowContents_Patch
	{
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var getMainRect = AccessTools.Method(typeof(Page), "GetMainRect");
			var adjust = AccessTools.Method(typeof(TechLevelSidebar), nameof(TechLevelSidebar.AdjustMainRect));
			var patched = false;
			foreach (var inst in instructions)
			{
				yield return inst;
				if (!patched && inst.Calls(getMainRect))
				{
					patched = true;
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Call, adjust);
				}
			}
			if (!patched)
			{
				Log.Warning("[Progression: Scenarios] Could not find GetMainRect in Page_SelectScenario.DoWindowContents; the tech level sidebar is disabled.");
			}
		}
	}

	[HarmonyPatch(typeof(Page), nameof(Page.InitialSize), MethodType.Getter)]
	public static class Page_InitialSize_Patch
	{
		public static void Postfix(Page __instance, ref Vector2 __result)
		{
			if (__instance is Page_SelectScenario && TechLevelSidebar.Active)
			{
				__result.x = Mathf.Min(__result.x + SidebarDrawer.Extra(TechLevelSidebar.Width, TechLevelSidebar.Gap), UI.screenWidth);
			}
		}
	}

	[HarmonyPatch(typeof(Window), nameof(Window.WindowUpdate))]
	public static class Window_WindowUpdate_Patch
	{
		public static void Postfix(Window __instance)
		{
			if (__instance is Page_SelectScenario && TechLevelSidebar.Active)
			{
				SidebarDrawer.SyncWindow(__instance);
			}
		}
	}

	public static class SidebarDrawer
	{
		public const float HandleWidth = 16f;
		public const float HandleGap = 4f;

		private const float ArrowSize = 12f;
		private const float Duration = 0.2f;

		private static bool open = false;
		private static float fromProgress = 0f;
		private static float toggledAt = -1000f;

		public static float Progress { get; private set; } = 0f;

		public static float Extra(float drawerWidth, float gap)
		{
			return HandleWidth + gap + (HandleGap + drawerWidth) * Progress;
		}

		public static void SyncWindow(Window window)
		{
			var t = Mathf.Clamp01((Time.realtimeSinceStartup - toggledAt) / Duration);
			Progress = Mathf.Lerp(fromProgress, open ? 1f : 0f, Mathf.SmoothStep(0f, 1f, t));
			var width = Mathf.Round(window.InitialSize.x);
			if (window.windowRect.width != width)
			{
				window.windowRect = new Rect(Mathf.Round((UI.screenWidth - width) / 2f), window.windowRect.y, width, window.windowRect.height);
			}
		}

		public static Rect DrawAndShift(Rect rect, float drawerWidth, float gap, string hideTip, string showTip, Action<Rect> drawDrawer)
		{
			DrawHandle(new Rect(rect.x, rect.y, HandleWidth, rect.height), hideTip, showTip);
			var drawerX = rect.x + HandleWidth + HandleGap * Progress;
			if (Progress >= 1f)
			{
				drawDrawer(new Rect(drawerX, rect.y, drawerWidth, rect.height));
			}
			else if (Progress > 0f)
			{
				var visible = drawerWidth * Progress;
				Widgets.BeginGroup(new Rect(drawerX, rect.y, visible, rect.height));
				drawDrawer(new Rect(visible - drawerWidth, 0f, drawerWidth, rect.height));
				Widgets.EndGroup();
			}
			var shift = Extra(drawerWidth, gap);
			rect.x += shift;
			rect.width -= shift;
			return rect;
		}

		private static void DrawHandle(Rect rect, string hideTip, string showTip)
		{
			Widgets.DrawBoxSolid(rect, new Color(1f, 1f, 1f, 0.025f));
			GUI.color = new Color(1f, 1f, 1f, 0.12f);
			Widgets.DrawBox(rect);
			GUI.color = Mouse.IsOver(rect) ? Color.white : new Color(1f, 1f, 1f, 0.6f);
			var arrowRect = new Rect(rect.center.x - ArrowSize / 2f, rect.center.y - ArrowSize / 2f, ArrowSize, ArrowSize);
			GUI.DrawTexture(arrowRect, open ? TexUI.ArrowTexRight : TexUI.ArrowTexLeft, ScaleMode.ScaleToFit);
			GUI.color = Color.white;
			Widgets.DrawHighlightIfMouseover(rect);
			TooltipHandler.TipRegion(rect, open ? hideTip : showTip);
			if (Widgets.ButtonInvisible(rect))
			{
				(open ? SoundDefOf.TabClose : SoundDefOf.TabOpen).PlayOneShotOnCamera();
				fromProgress = Progress;
				open = !open;
				toggledAt = Time.realtimeSinceStartup;
			}
		}
	}

}
