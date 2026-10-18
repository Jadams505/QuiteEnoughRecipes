using QuiteEnoughRecipes.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using Terraria.ID;
using Terraria.Localization;
using static Terraria.ID.ContentSamples.CreativeHelper;

namespace QuiteEnoughRecipes.ModFilters;

public class MiscItemFiltersHandler : QERFilterHandler
{
	public override LocalizedText FilterGroupText { get; }
		= Language.GetText("Mods.QuiteEnoughRecipes.Filters.MiscItemFilters");

	public override IEnumerable<IFilterTab<IIngredient>> GetFilters(IEnumerable<IIngredient> items)
	{
		static bool IsTile(IIngredient ingredient) => ingredient is ItemIngredient itemIngredient && IngredientOptions.IsTile(itemIngredient);
		yield return new SimpleFilterTab<IIngredient>(Language.GetText("Mods.QuiteEnoughRecipes.Filters.IsTile"), ItemID.StoneBlock, IsTile);

		static bool IsCraftingStation(IIngredient i) => i is ItemIngredient itemIng && IngredientOptions.IsInGroup(itemIng, ItemGroup.CraftingObjects);
		yield return new SimpleFilterTab<IIngredient>(Language.GetText("Mods.QuiteEnoughRecipes.Filters.IsCraftingStation"), ItemID.Furnace, IsCraftingStation);
	}

	public override bool ShouldShowInUI(IIngredient obj)
	{
		return obj is ItemIngredient;
	}
}
