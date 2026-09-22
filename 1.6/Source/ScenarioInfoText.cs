using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace ProgressionScenarios
{
	public static class ScenarioInfoText
	{
		private const string StartsWithTag = "PlayerStartsWith";
		private const string EntryPrefix = "   -";
		private const string RandomSeedBundlesPart = "ScenPart_RandomSeedBundles";
		private const float IconGap = 3f;

		private static readonly Dictionary<string, EntryIcon> entryIcons = new Dictionary<string, EntryIcon>();
		private static Scenario cachedScenario;
		private static string cachedText;

		public static float Height(Scenario scen, string text, float width)
		{
			return Layout(new Rect(0f, 0f, width, 0f), scen, text, false);
		}

		public static void Draw(Rect rect, Scenario scen, string text)
		{
			Layout(rect, scen, text, true);
		}

		private static float Layout(Rect rect, Scenario scen, string text, bool draw)
		{
			Recache(scen, text);
			var padding = Text.CurFontStyle.padding;
			var header = ScenPart_StartingThing_Defined.PlayerStartWithIntro + ":";
			var iconSize = ContentHeight(" ", rect.width, padding);
			var iconX = rect.x + padding.left + Text.CalcSize(EntryPrefix).x - padding.horizontal + IconGap;
			var entryRectX = iconX + iconSize + IconGap - padding.left;
			var entryWidth = rect.xMax - entryRectX;
			var y = rect.y + padding.top;
			var inStartsWith = false;
			foreach (var line in text.Replace("\r", "").Split('\n'))
			{
				var icon = default(EntryIcon);
				var hasIcon = false;
				if (line == header) inStartsWith = true;
				else if (!line.StartsWith(EntryPrefix)) inStartsWith = false;
				else if (inStartsWith) hasIcon = entryIcons.TryGetValue(line.Substring(EntryPrefix.Length), out icon);

				if (!hasIcon)
				{
					var height = ContentHeight(line, rect.width, padding);
					if (draw) Widgets.Label(new Rect(rect.x, y - padding.top, rect.width, height + padding.vertical), line);
					y += height;
					continue;
				}

				var entry = line.Substring(EntryPrefix.Length);
				var entryHeight = ContentHeight(entry, entryWidth, padding);
				if (draw)
				{
					Widgets.Label(new Rect(rect.x, y - padding.top, rect.width, iconSize + padding.vertical), EntryPrefix);
					icon.Draw(new Rect(iconX, y, iconSize, iconSize));
					Widgets.Label(new Rect(entryRectX, y - padding.top, entryWidth, entryHeight + padding.vertical), entry);
				}
				y += entryHeight;
			}
			return y + padding.bottom - rect.y;
		}

		private static float ContentHeight(string text, float width, RectOffset padding)
		{
			return Text.CalcHeight(text.Length == 0 ? " " : text, width) - padding.vertical;
		}

		private static void Recache(Scenario scen, string text)
		{
			if (scen == cachedScenario && text == cachedText) return;
			cachedScenario = scen;
			cachedText = text;
			entryIcons.Clear();
			foreach (var part in scen.AllParts)
			{
				var icon = IconFor(part);
				if (!icon.HasValue) continue;
				foreach (var entry in part.GetSummaryListEntries(StartsWithTag))
				{
					entryIcons[entry] = icon.Value;
				}
			}
		}

		private static EntryIcon? IconFor(ScenPart part)
		{
			switch (part)
			{
				case ScenPart_ThingCount thingCount when thingCount.thingDef != null:
					return new EntryIcon(thingCount.thingDef, thingCount.stuff, null);
				case ScenPart_StartingAnimal animal when animal.animalKind != null:
					return new EntryIcon(animal.animalKind, null, null);
				case ScenPart_StartingAnimal _:
					return TextureIcon("UI/Icons/Animal");
				case ScenPart_StartingMech mech when mech.mechKind != null:
					return new EntryIcon(mech.mechKind, null, null);
			}
			return part.GetType().Name == RandomSeedBundlesPart ? TextureIcon("SeedBundle") : null;
		}

		private static EntryIcon? TextureIcon(string path)
		{
			var texture = ContentFinder<Texture2D>.Get(path, false);
			if (texture == null) return null;
			return new EntryIcon(null, null, texture);
		}

		private readonly struct EntryIcon
		{
			private readonly Def def;
			private readonly ThingDef stuff;
			private readonly Texture2D texture;

			public EntryIcon(Def def, ThingDef stuff, Texture2D texture)
			{
				this.def = def;
				this.stuff = stuff;
				this.texture = texture;
			}

			public void Draw(Rect rect)
			{
				if (texture != null) GUI.DrawTexture(rect, texture);
				else Widgets.DefIcon(rect, def, stuff);
			}
		}
	}
}
