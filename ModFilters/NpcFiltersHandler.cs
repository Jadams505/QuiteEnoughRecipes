using QuiteEnoughRecipes.ModAPI;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;
using Terraria.Localization;

namespace QuiteEnoughRecipes.ModFilters;

public class NpcFiltersHandler : QERFilterHandler
{
	public override LocalizedText FilterGroupText { get; }
		= Language.GetText("Mods.QuiteEnoughRecipes.OptionGroups.NPCFilters.Name");

	public override IEnumerable<IFilterTab<IIngredient>> GetFilters()
	{
		yield return new SimpleIngredientFilterTab<NPCIngredient>(Language.GetText("Mods.QuiteEnoughRecipes.OptionGroups.NPCFilters.IsBoss"), ItemID.SlimeCrown, IngredientOptions.IsBoss);

		yield return new SimpleIngredientFilterTab<NPCIngredient>(Language.GetText("Mods.QuiteEnoughRecipes.OptionGroups.NPCFilters.IsTownNPC"), ItemID.GoldCoin, IngredientOptions.IsTownNPC);
	}

	public override bool Applies(IEnumerable<IIngredient> items)
	{
		return items.Any(i => i is NPCIngredient);
	}
}

