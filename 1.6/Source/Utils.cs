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
		private const float RecommendedFlashDuration = 1.2f;

		private static Dictionary<Scenario, ScenarioDef> scenarioDefCache = new Dictionary<Scenario, ScenarioDef>();
		private static Dictionary<Scenario, Texture2D> scenarioIconCache = new Dictionary<Scenario, Texture2D>();
		private static Dictionary<TechLevel, int> scenarioCountCache = new Dictionary<TechLevel, int>();
		private static Dictionary<string, bool> modActiveCache = new Dictionary<string, bool>();

		public static GameComponent_ProgressionScenarios ProgressionComponent =>
			Current.Game?.GetComponent<GameComponent_ProgressionScenarios>();

		public static bool ThemingModActive => ModActive(ThemingModPackageId);

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
			scenarioCountCache.Clear();
			foreach (var scen in ScenarioLister.AllScenarios())
			{
				var techLevel = scen?.playerFaction?.factionDef?.techLevel ?? TechLevel.Undefined;
				scenarioCountCache.TryGetValue(techLevel, out var count);
				scenarioCountCache[techLevel] = count + 1;
			}
		}

		public static int ScenarioCount(this TechLevel techLevel)
		{
			return scenarioCountCache.TryGetValue(techLevel, out var count) ? count : 0;
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

		public static void DrawRecommendedLabel(Rect rect, float openedAt, TextAnchor anchor)
		{
			Text.Font = GameFont.Small;
			Text.Anchor = anchor;
			GUI.color = Color.Lerp(ColoredText.SubtleGrayColor, Color.white, RecommendedFlashIntensity(openedAt));
			Widgets.Label(rect, "PS_Recommended".Translate());
			GUI.color = Color.white;
			Text.Anchor = TextAnchor.UpperLeft;
		}

		private static float RecommendedFlashIntensity(float openedAt)
		{
			var t = (Time.realtimeSinceStartup - openedAt) / RecommendedFlashDuration;
			if (t <= 0f || t >= 1f) return 0f;
			return 0.5f - (0.5f * Mathf.Cos(t * 2f * Mathf.PI));
		}
	}
}
