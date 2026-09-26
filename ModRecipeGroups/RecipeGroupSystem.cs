using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace QuiteEnoughRecipes.ModRecipeGroups;

public class RecipeGroupSystem : ModSystem
{
	public static IRecipeGroup AllFishingRods = null!;

	// Not actually adding these as official recipe groups
	public override void AddRecipeGroups()
	{
		AllFishingRods = new SimpleRecipeGroup
		(
			() => Language.GetOrRegister("Mods.QuiteEnoughRecipes.RecipeGroups.AllFishingRods").Value,
			ContentSamples.ItemsByType.Where(p => p.Value.fishingPole > 0).Select(p => p.Key).ToHashSet()
		);
	}
}