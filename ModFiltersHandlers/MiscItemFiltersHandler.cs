using QuiteEnoughRecipes.ModAPI;
using QuiteEnoughRecipes.ModFiltersHandlers;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;
using Terraria.Localization;
using static Terraria.ID.ContentSamples.CreativeHelper;

namespace QuiteEnoughRecipes.ModFilters;

public class MiscItemFiltersHandler : QERFilterHandler
{
	public override LocalizedText FilterGroupText { get; }
		= Language.GetText("Mods.QuiteEnoughRecipes.OptionGroups.ItemFilters.Misc.Name");

	public override IEnumerable<IFilterTab<IIngredient>> GetFilters()
	{
		yield return new SimpleIngredientFilterTab<ItemIngredient>(Language.GetText("Mods.QuiteEnoughRecipes.OptionGroups.ItemFilters.Misc.IsTile"), ItemID.StoneBlock, IngredientOptions.IsTile);

		yield return new SimpleIngredientFilterTab<ItemIngredient>(Language.GetText("Mods.QuiteEnoughRecipes.OptionGroups.ItemFilters.Misc.IsCraftingStation"), ItemID.Furnace, itemIng => IngredientOptions.IsInGroup(itemIng, ItemGroup.CraftingObjects));
	}

	public override bool Applies(IEnumerable<IIngredient> items)
	{
		return items.Any(i => i is ItemIngredient);
	}
}
