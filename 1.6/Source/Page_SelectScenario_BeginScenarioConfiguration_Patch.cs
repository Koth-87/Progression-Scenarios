using HarmonyLib;
using RimWorld;
using Verse;

namespace ProgressionScenarios
{
	[HarmonyPatch(typeof(Page_SelectScenario), nameof(Page_SelectScenario.BeginScenarioConfiguration))]
	public static class Page_SelectScenario_BeginScenarioConfiguration_Patch
	{
		public static void Postfix(Page originPage)
		{
			var techLevel = ScenarioLister_ScenariosInCategory_Patch.selectedLevel;
			if (techLevel == TechLevel.Undefined || techLevel == TechLevel.Animal) return;

			var comp = Utils.ProgressionComponent;
			if (comp != null)
			{
				comp.startingTechLevel = techLevel;
				comp.unlockPreviousEras = false;
			}

			var firstConfigPage = originPage?.next;
			if (firstConfigPage == null) return;

			var page = new Page_UnlockPreviousEras { techLevel = techLevel };
			originPage.next = page;
			page.prev = originPage;
			page.next = firstConfigPage;
			firstConfigPage.prev = page;
		}
	}
}
