using HarmonyLib;
using RimWorld;
using Verse;

namespace ProgressionScenarios
{
	[HarmonyPatch(typeof(ResearchUtility), nameof(ResearchUtility.ApplyPlayerStartingResearch))]
	public static class ResearchUtility_ApplyPlayerStartingResearch_Patch
	{
		public static void Postfix()
		{
			var comp = Utils.ProgressionComponent;
			if (comp == null || !comp.unlockPreviousEras) return;
			if (comp.startingTechLevel == TechLevel.Undefined) return;

			foreach (var project in DefDatabase<ResearchProjectDef>.AllDefsListForReading)
			{
				if (project.knowledgeCategory != null) continue;
				if (project.techLevel == TechLevel.Undefined) continue;
				if (project.techLevel >= comp.startingTechLevel) continue;
				if (project.IsFinished) continue;
				Find.ResearchManager.FinishProject(project, false, null, false);
			}
		}
	}
}
