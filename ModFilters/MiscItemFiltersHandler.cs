using QuiteEnoughRecipes.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.IO;
using static Terraria.ID.ContentSamples.CreativeHelper;

namespace QuiteEnoughRecipes.ModFilters;

public class MiscItemFiltersHandler : QERFilterHandler
{
	public override LocalizedText FilterGroupText { get; }
		= Language.GetText("Mods.QuiteEnoughRecipes.OptionGroups.ItemFilters.Misc.Name");

	public override IEnumerable<IFilterTab<IIngredient>> GetFilters(IEnumerable<IIngredient> items)
	{
		yield return new SimpleIngredientFilterTab<ItemIngredient>(Language.GetText("Mods.QuiteEnoughRecipes.OptionGroups.ItemFilters.Misc.IsTile"), ItemID.StoneBlock, IngredientOptions.IsTile);

		yield return new SimpleIngredientFilterTab<ItemIngredient>(Language.GetText("Mods.QuiteEnoughRecipes.OptionGroups.ItemFilters.Misc.IsCraftingStation"), ItemID.Furnace, itemIng => IngredientOptions.IsInGroup(itemIng, ItemGroup.CraftingObjects));
	}

	public override bool ShouldShowInUI(IIngredient obj)
	{
		return obj is ItemIngredient;
	}
}
