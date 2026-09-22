using System;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace ProgressionScenarios
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class HotSwappableAttribute : Attribute
	{
	}

	[HotSwappable]
	[StaticConstructorOnStartup]
	public class Page_SelectTechLevel : Page
	{
		private const float TitleAdvance = 27f;
		private const float CountGap = 6f;
		private const float IconStripGap = 5f;
		private const float IconStripSize = 20f;
		private const int IconStripMax = 5;

		private float openedAt;

		public override string PageTitle => "PS_ChooseTechLevel".Translate();
		public override Vector2 InitialSize => new Vector2(StandardSize.x + SidebarDrawer.HandleWidth + TechLevelSidebar.Gap, StandardSize.y);
		public override void PreOpen()
		{
			base.PreOpen();
			ScenarioLister.MarkDirty();
			Utils.RecacheScenarioCounts();
			openedAt = Time.realtimeSinceStartup;
		}

		public override void DoWindowContents(Rect rect)
		{
			DrawPageTitle(rect);
			var mainRect = GetMainRect(rect);
			Widgets.BeginGroup(mainRect);

			var boxWidth = 310f;
			var boxHeight = 140f;
			var spacingX = 20f;
			var spacingY = 20f;

			var totalWidth = 3f * boxWidth + 2f * spacingX;
			var startX = (mainRect.width - totalWidth) / 2f;
			var startY = Mathf.Max(0f, (mainRect.height - (3f * boxHeight + 2f * spacingY)) / 2f);

			var animalRect = new Rect((mainRect.width - boxWidth) / 2f, startY, boxWidth, boxHeight);
			DrawTechLevelBox(animalRect, TechLevel.Animal);

			var row2Y = startY + boxHeight + spacingY;
			DrawTechLevelBox(new Rect(startX, row2Y, boxWidth, boxHeight), TechLevel.Neolithic);
			DrawTechLevelBox(new Rect(startX + boxWidth + spacingX, row2Y, boxWidth, boxHeight), TechLevel.Medieval);
			DrawTechLevelBox(new Rect(startX + 2f * (boxWidth + spacingX), row2Y, boxWidth, boxHeight), TechLevel.Industrial);

			var row3Y = row2Y + boxHeight + spacingY;
			DrawTechLevelBox(new Rect(startX, row3Y, boxWidth, boxHeight), TechLevel.Spacer);
			DrawTechLevelBox(new Rect(startX + boxWidth + spacingX, row3Y, boxWidth, boxHeight), TechLevel.Ultra);
			DrawTechLevelBox(new Rect(startX + 2f * (boxWidth + spacingX), row3Y, boxWidth, boxHeight), TechLevel.Archotech);

			Widgets.EndGroup();

			DoBottomButtons(rect, showNext: false);
		}

		private void DrawTechLevelBox(Rect boxRect, TechLevel techLevel)
		{
			var recommended = techLevel == TechLevel.Animal && Utils.ThemingModActive;
			Widgets.DrawBox(boxRect, 1);
			Widgets.DrawHighlightIfMouseover(boxRect);
			if (Widgets.ButtonInvisible(boxRect))
			{
				ScenarioLister_ScenariosInCategory_Patch.selectedLevel = techLevel;
				if (ScenarioLister.ScenariosInCategory(ScenarioCategory.FromDef).Any() ||
				    ScenarioLister.ScenariosInCategory(ScenarioCategory.CustomLocal).Any() ||
				    ScenarioLister.ScenariosInCategory(ScenarioCategory.SteamWorkshop).Any())
				{
					this.Close();
					var page = new Page_SelectScenario();
					page.prev = this;
					Find.WindowStack.Add(page);
				}
				else
				{
					ScenarioLister_ScenariosInCategory_Patch.selectedLevel = TechLevel.Undefined;
					Messages.Message("PS_NoScenariosForTechLevel".Translate(), MessageTypeDefOf.RejectInput, historical: false);
				}
			}

			var iconSize = boxRect.height * 0.8f;
			var iconRect = new Rect(boxRect.x + 15f, boxRect.y + (boxRect.height - iconSize) / 2f, iconSize, iconSize);
			var tex = ContentFinder<Texture2D>.Get("UI/Icons/" + techLevel.ToString(), false);
			if (tex != null) GUI.DrawTexture(iconRect, tex);

			var textRect = new Rect(iconRect.xMax + 15f, boxRect.y, boxRect.width - iconRect.width - 45f, boxRect.height);
			var desc = ("PS_TechLevelDesc_" + techLevel.ToString()).Translate();
			var count = techLevel.ScenarioCount();
			var countLabel = count == 1 ? "PS_SelectFromScenario".Translate() : "PS_SelectFromScenarios".Translate(count);

			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Tiny;
			var descHeight = Text.CalcHeight(desc, textRect.width);
			var countHeight = Text.CalcHeight(countLabel, textRect.width);
			var hasIcons = techLevel.Scenarios().Any(s => s.GetIcon() != null);
			var blockHeight = TitleAdvance + descHeight + CountGap + countHeight + (hasIcons ? IconStripGap + IconStripSize : 0f);
			var curY = textRect.y + (textRect.height - blockHeight) / 2f;

			Text.Font = GameFont.Medium;
			Widgets.Label(new Rect(textRect.x, curY, textRect.width, 32f), techLevel.GetTechLevelName());
			curY += TitleAdvance;
			Text.Font = GameFont.Tiny;
			Widgets.Label(new Rect(textRect.x, curY, textRect.width, descHeight), desc);
			curY += descHeight + CountGap;
			GUI.color = ColoredText.SubtleGrayColor;
			Widgets.Label(new Rect(textRect.x, curY, textRect.width, countHeight), countLabel);
			GUI.color = Color.white;
			curY += countHeight;
			if (hasIcons)
			{
				Utils.DrawScenarioIconStrip(new Rect(textRect.x, curY + IconStripGap, textRect.width, IconStripSize), techLevel.Scenarios(), IconStripSize, IconStripMax);
			}
			Utils.ScenarioListTip(boxRect, techLevel);

			if (recommended) Utils.DrawRecommendedTag(boxRect, openedAt, false);

			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Small;
		}
	}
}
