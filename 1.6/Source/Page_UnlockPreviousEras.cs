using RimWorld;
using UnityEngine;
using Verse;

namespace ProgressionScenarios
{
	[HotSwappable]
	public class Page_UnlockPreviousEras : Page
	{
		private const float CheckboxHeight = 32f;
		private const float DescriptionGap = 14f;

		public TechLevel techLevel = TechLevel.Undefined;
		private bool unlockPreviousEras;

		public override string PageTitle => "PS_UnlockPreviousErasTitle".Translate();
		public override Vector2 InitialSize => new Vector2(640f, 400f);

		public override void DoWindowContents(Rect rect)
		{
			DrawPageTitle(rect);
			var mainRect = GetMainRect(rect);

			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperLeft;
			var checkboxRect = new Rect(mainRect.x, mainRect.y, mainRect.width, CheckboxHeight);
			Widgets.CheckboxLabeled(checkboxRect, "PS_UnlockPreviousEras".Translate(), ref unlockPreviousEras, false, null, null, true);

			var comp = Utils.ProgressionComponent;
			if (comp != null)
			{
				comp.startingTechLevel = techLevel;
				comp.unlockPreviousEras = unlockPreviousEras;
			}

			var description = "PS_UnlockPreviousErasDesc".Translate(techLevel.GetTechLevelName());
			var descriptionRect = new Rect(mainRect.x, checkboxRect.yMax + DescriptionGap, mainRect.width, mainRect.height - CheckboxHeight - DescriptionGap);
			GUI.color = ColoredText.SubtleGrayColor;
			Widgets.Label(descriptionRect, description);
			GUI.color = Color.white;

			DoBottomButtons(rect);
		}
	}
}
