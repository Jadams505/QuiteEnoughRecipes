using QuiteEnoughRecipes.ModAPI.Sorts;
using QuiteEnoughRecipes.ModFiltersHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria.ID;
using Terraria.Localization;
using static QuiteEnoughRecipes.IngredientOptions;

namespace QuiteEnoughRecipes.ModSortHandlers;

public class NpcSortHandler : QERSortHandler
{
	public const string GroupKey = "Mods.QuiteEnoughRecipes.OptionGroups.NPCSorts";

	public override LocalizedText SortGroupText { get; }
		= Language.GetText($"{GroupKey}.Name");

	public override bool Applies(IEnumerable<IIngredient> items)
	{
		return items.Any(i => i is NPCIngredient);
	}

	public override IEnumerable<ISortTab<IIngredient>> GetSorts()
	{
		yield return new SimpleIngredientSortTab<NPCIngredient>(Language.GetText($"{GroupKey}.ByBestiary"), ItemID.TreeStatue, ByBestiary);
		yield return new SimpleIngredientSortTab<NPCIngredient>(Language.GetText($"{GroupKey}.ByID"), ItemID.AlphabetStatue1, ByID);
		yield return new SimpleIngredientSortTab<NPCIngredient>(Language.GetText($"{GroupKey}.ByName"), ItemID.AlphabetStatueA, ByName);
		yield return new SimpleIngredientSortTab<NPCIngredient>(Language.GetText($"{GroupKey}.ByRarity"), ItemID.StarStatue, ByRarity);
	}
}
