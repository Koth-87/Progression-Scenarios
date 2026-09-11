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
		private const float RecommendedReserve = 26f;

		private float openedAt;

		public override string PageTitle => "PS_ChooseTechLevel".Translate();
		public override Vector2 InitialSize
		{
			get
			{
				var size = base.InitialSize;
				return new Vector2(size.x, size.y - 75);
			}
		}
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
			var startY = 20f;

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

			var y = rect.y + rect.height - 38f;
			var label = "Back".Translate();
			var backSize = 200f;
			if ((Widgets.ButtonText(new Rect((rect.width / 2f) - (backSize / 2f), y, backSize, BottomButSize.y), label) || KeyBindingDefOf.Cancel.KeyDownEvent) && CanDoBack())
			{
				DoBack();
			}
		}

		private void DrawTechLevelBox(Rect boxRect, TechLevel techLevel)
		{
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

			var recommended = techLevel == TechLevel.Animal && Utils.ThemingModActive;
			var textRect = new Rect(iconRect.xMax + 15f, boxRect.y, boxRect.width - iconRect.width - 45f, boxRect.height);
			var desc = ("PS_TechLevelDesc_" + techLevel.ToString()).Translate();
			var count = techLevel.ScenarioCount();
			var countLabel = count == 1 ? "PS_SelectFromScenario".Translate() : "PS_SelectFromScenarios".Translate(count);

			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Tiny;
			var descHeight = Text.CalcHeight(desc, textRect.width);
			var countHeight = Text.CalcHeight(countLabel, textRect.width);
			var blockHeight = TitleAdvance + descHeight + CountGap + countHeight;
			var curY = textRect.y + (textRect.height - (recommended ? RecommendedReserve : 0f) - blockHeight) / 2f;

			Text.Font = GameFont.Medium;
			Widgets.Label(new Rect(textRect.x, curY, textRect.width, 32f), techLevel.GetTechLevelName());
			curY += TitleAdvance;
			Text.Font = GameFont.Tiny;
			Widgets.Label(new Rect(textRect.x, curY, textRect.width, descHeight), desc);
			curY += descHeight + CountGap;
			GUI.color = ColoredText.SubtleGrayColor;
			Widgets.Label(new Rect(textRect.x, curY, textRect.width, countHeight), countLabel);
			GUI.color = Color.white;

			if (recommended) Utils.DrawRecommendedLabel(new Rect(boxRect.x, boxRect.yMax - RecommendedReserve, boxRect.width, 24f), openedAt, TextAnchor.UpperCenter);

			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Small;
		}
	}
}
