using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;

namespace ProgressionScenarios
{
	[HarmonyPatch(typeof(ScenarioLister), "ScenariosInCategory")]
	public static class ScenarioLister_ScenariosInCategory_Patch
	{
		public static TechLevel selectedLevel;
		public static IEnumerable<Scenario> Postfix(IEnumerable<Scenario> result)
		{
			var filtered = new List<Scenario>();
			foreach (var scenario in result)
			{
				if (selectedLevel == scenario.playerFaction?.factionDef?.techLevel)
				{
					filtered.Add(scenario);
				}
			}
			return filtered.OrderBy(s => {
				var def = s.GetDef();
				return def?.GetModExtension<ScenarioExtension>()?.displayOrder ?? 20f;
			});
		}
	}
}
