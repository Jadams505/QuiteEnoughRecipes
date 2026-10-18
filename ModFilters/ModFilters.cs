using System;
using System.Collections.Generic;
using System.Text;
using Terraria;
using Terraria.ID;
using Terraria.Localization;

namespace QuiteEnoughRecipes.ModFilters;

public static class ModFilters
{
	public static readonly IFilterTab<IIngredient> IsTile = new SimpleFilterTab<IIngredient>
	(
		HoverName: Language.GetText("Mods.QuiteEnoughRecipes.Filters.ItemIn"),
		TabElement: ITab.ItemIcon(new Item(ItemID.IronBar)),
		Filter: f => f is ItemIngredient itemIngredient && IngredientOptions.IsTile(itemIngredient)
	);
}
