using QuiteEnoughRecipes.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.IO;
using static Terraria.ID.ContentSamples.CreativeHelper;

namespace QuiteEnoughRecipes.ModFilters;

public class NpcFiltersHandler : QERFilterHandler
{
	public override LocalizedText FilterGroupText { get; }
		= Language.GetText("Mods.QuiteEnoughRecipes.OptionGroups.NPCFilters.Name");

	public override IEnumerable<IFilterTab<IIngredient>> GetFilters(IEnumerable<IIngredient> items)
	{
		yield return new SimpleIngredientFilterTab<NPCIngredient>(Language.GetText("Mods.QuiteEnoughRecipes.OptionGroups.NPCFilters.IsBoss"), ItemID.SlimeCrown, IngredientOptions.IsBoss);

		yield return new SimpleIngredientFilterTab<NPCIngredient>(Language.GetText("Mods.QuiteEnoughRecipes.OptionGroups.NPCFilters.IsTownNPC"), ItemID.GoldCoin, IngredientOptions.IsTownNPC);
	}

	public override bool ShouldShowInUI(IIngredient obj)
	{
		return obj is NPCIngredient;
	}
}

