using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace ProgressionScenarios
{
	public static class Utils
	{
		public const string ThemingModPackageId = "ferny.themingformodpack";
		public const string ScenarioSearchModPackageId = "sk.scenariosearch";
		public const float ScenarioSearchListOffset = 8f;
		private const float RecommendedFlashDuration = 1.2f;

		private static Dictionary<Scenario, ScenarioDef> scenarioDefCache = new Dictionary<Scenario, ScenarioDef>();
		private static Dictionary<Scenario, Texture2D> scenarioIconCache = new Dictionary<Scenario, Texture2D>();
		private static Dictionary<TechLevel, List<Scenario>> scenariosByLevelCache = new Dictionary<TechLevel, List<Scenario>>();
		private static Dictionary<string, bool> modActiveCache = new Dictionary<string, bool>();

		public static GameComponent_ProgressionScenarios ProgressionComponent =>
			Current.Game?.GetComponent<GameComponent_ProgressionScenarios>();

		public static bool ThemingModActive => ModActive(ThemingModPackageId);

		public static bool ScenarioSearchModActive => ModActive(ScenarioSearchModPackageId);

		public static bool ModActive(string packageId)
		{
			if (!modActiveCache.TryGetValue(packageId, out var active))
			{
				active = ModLister.GetActiveModWithIdentifier(packageId, true) != null;
				modActiveCache[packageId] = active;
			}
			return active;
		}

		public static ScenarioDef GetDef(this Scenario scen)
		{
			if (scenarioDefCache.TryGetValue(scen, out var cached)) return cached;
			var def = DefDatabase<ScenarioDef>.AllDefsListForReading.FirstOrDefault(d => d.scenario == scen);
			scenarioDefCache[scen] = def;
			return def;
		}

		public static Texture2D GetIcon(this Scenario scen)
		{
			if (scenarioIconCache.TryGetValue(scen, out var cached)) return cached;
			Texture2D icon = null;
			var def = scen.GetDef();
			if (def != null)
			{
				var ext = def.GetModExtension<ScenarioExtension>();
				if (ext != null && !ext.iconPath.NullOrEmpty()) icon = ContentFinder<Texture2D>.Get(ext.iconPath, false);
				if (icon == null && def.modContentPack != null)
				{
					foreach (var expansionDef in DefDatabase<ExpansionDef>.AllDefs)
					{
						if (expansionDef.linkedMod == def.modContentPack.PackageId && !expansionDef.iconPath.NullOrEmpty())
						{
							icon = ContentFinder<Texture2D>.Get(expansionDef.iconPath, false);
							break;
						}
					}
					if (icon == null)
					{
						icon = ModLister.GetModWithIdentifier(def.modContentPack.PackageId)?.Icon;
					}
				}
			}
			scenarioIconCache[scen] = icon;
			return icon;
		}

		public static void RecacheScenarioCounts()
		{
			scenariosByLevelCache.Clear();
			foreach (var scen in ScenarioLister.AllScenarios())
			{
				if (scen == null || !scen.showInUI) continue;
				var techLevel = scen.playerFaction?.factionDef?.techLevel ?? TechLevel.Undefined;
				if (!scenariosByLevelCache.TryGetValue(techLevel, out var list))
				{
					list = new List<Scenario>();
					scenariosByLevelCache[techLevel] = list;
				}
				list.Add(scen);
			}
			foreach (var list in scenariosByLevelCache.Values)
			{
				list.SortBy(s => s.GetDef()?.GetModExtension<ScenarioExtension>()?.displayOrder ?? 20f);
			}
		}

		public static List<Scenario> Scenarios(this TechLevel techLevel)
		{
			return scenariosByLevelCache.TryGetValue(techLevel, out var list) ? list : new List<Scenario>();
		}

		public static int ScenarioCount(this TechLevel techLevel)
		{
			return techLevel.Scenarios().Count;
		}

		public static void DrawScenarioIconStrip(Rect rect, List<Scenario> scenarios, float iconSize, int maxIcons)
		{
			var icons = scenarios.Select(s => s.GetIcon()).Where(t => t != null).Distinct().ToList();
			var shown = Mathf.Min(icons.Count, maxIcons);
			var x = rect.x;
			var y = rect.center.y - iconSize / 2f;
			for (var i = 0; i < shown; i++)
			{
				var iconRect = new Rect(x, y, iconSize, iconSize);
				GUI.DrawTexture(iconRect, icons[i], ScaleMode.ScaleToFit);
				x += iconSize + 2f;
			}
			if (icons.Count > shown)
			{
				Text.Font = GameFont.Tiny;
				Text.Anchor = TextAnchor.MiddleLeft;
				GUI.color = ColoredText.SubtleGrayColor;
				Widgets.Label(new Rect(x + 2f, rect.y, rect.xMax - x - 2f, rect.height), "+" + (icons.Count - shown));
				GUI.color = Color.white;
				Text.Anchor = TextAnchor.UpperLeft;
			}
		}

		public static void ScenarioListTip(Rect rect, TechLevel techLevel, string description = null)
		{
			var scenarios = techLevel.Scenarios();
			if (scenarios.Count == 0) return;
			TooltipHandler.TipRegion(rect, () => techLevel.GetTechLevelName().Colorize(ColoredText.TipSectionTitleColor)
				+ (description.NullOrEmpty() ? "" : "\n" + description + "\n")
				+ "\n" + scenarios.Select(s => "  - " + s.name).ToLineList(), (int)techLevel ^ 0x5ce4);
		}

		public static string GetTechLevelName(this TechLevel techLevel)
		{
			if (techLevel == TechLevel.Ultra) return "PS_Ultratech".Translate();
			return techLevel.ToStringHuman().CapitalizeFirst();
		}

		public static bool IsRecommended(this Scenario scen)
		{
			var mods = scen.GetDef()?.GetModExtension<ScenarioExtension>()?.recommendedWithMods;
			return !mods.NullOrEmpty() && mods.All(ModActive);
		}

		public static readonly Color RecommendedColor = Color.white;

		public static void DrawRecommendedTag(Rect boxRect, float openedAt, bool goldBorder = true)
		{
			Text.Font = GameFont.Tiny;
			Text.Anchor = TextAnchor.MiddleCenter;
			if (goldBorder)
			{
				GUI.color = RecommendedColor;
				Widgets.DrawBox(boxRect);
				GUI.color = Color.white;
			}
			var label = "PS_Recommended".Translate();
			var size = Text.CalcSize(label);
			var tagRect = new Rect(boxRect.xMax - size.x - 14f, boxRect.y - 9f, size.x + 14f, 18f);
			Widgets.DrawBoxSolid(tagRect, Widgets.WindowBGFillColor);
			GUI.color = Color.Lerp(RecommendedColor, Color.white, RecommendedFlashIntensity(openedAt));
			Widgets.DrawBox(tagRect);
			Widgets.Label(tagRect, label);
			GUI.color = Color.white;
			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Small;
		}

		private static float RecommendedFlashIntensity(float openedAt)
		{
			var t = (Time.realtimeSinceStartup - openedAt) / RecommendedFlashDuration;
			if (t <= 0f || t >= 1f) return 0f;
			return 0.5f - (0.5f * Mathf.Cos(t * 2f * Mathf.PI));
		}
	}
}
