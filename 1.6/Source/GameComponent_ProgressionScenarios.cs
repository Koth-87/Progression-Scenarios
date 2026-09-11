using RimWorld;
using Verse;

namespace ProgressionScenarios
{
	public class GameComponent_ProgressionScenarios : GameComponent
	{
		public bool unlockPreviousEras;
		public TechLevel startingTechLevel = TechLevel.Undefined;

		public GameComponent_ProgressionScenarios(Game game)
		{
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref unlockPreviousEras, "unlockPreviousEras", false);
			Scribe_Values.Look(ref startingTechLevel, "startingTechLevel", TechLevel.Undefined);
		}
	}
}
