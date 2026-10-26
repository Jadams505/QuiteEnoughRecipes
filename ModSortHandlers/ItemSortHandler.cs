using QuiteEnoughRecipes.ModAPI;
using QuiteEnoughRecipes.ModFiltersHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria.ID;
using Terraria.Localization;
using static QuiteEnoughRecipes.IngredientOptions;

namespace QuiteEnoughRecipes.ModSortHandlers;

public class ItemSortHandler : QERSortHandler
{
	public const string GroupKey = "Mods.QuiteEnoughRecipes.OptionGroups.ItemSorts";

	public override LocalizedText SortGroupText { get; }
		= Language.GetText($"{GroupKey}.Name");

	public override bool Applies(IEnumerable<IIngredient> items)
	{
		return items.Any(i => i is ItemIngredient);
	}

	public override IEnumerable<ISortTab<IIngredient>> GetSorts()
	{
		yield return new SimpleIngredientSortTab<ItemIngredient>(Language.GetText($"{GroupKey}.ByCreative"), ItemID.TreeStatue, ByCreative);
		yield return new SimpleIngredientSortTab<ItemIngredient>(Language.GetText($"{GroupKey}.ByID"), ItemID.AlphabetStatue1, ByID);
	}
}
