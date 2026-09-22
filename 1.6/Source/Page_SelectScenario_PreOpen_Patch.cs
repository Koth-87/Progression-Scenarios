using HarmonyLib;
using RimWorld;
using UnityEngine;

namespace ProgressionScenarios
{
	[HarmonyPatch(typeof(Page_SelectScenario), nameof(Page_SelectScenario.PreOpen))]
	public static class Page_SelectScenario_PreOpen_Patch
	{
		public static float openedAt;

		public static void Postfix()
		{
			openedAt = Time.realtimeSinceStartup;
			Utils.RecacheScenarioCounts();
		}
	}
}
