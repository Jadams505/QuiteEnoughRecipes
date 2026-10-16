using QuiteEnoughRecipes.ModIngredients;
using QuiteEnoughRecipes.ModRecipeHandlers;
using QuiteEnoughRecipes.ModUIElements;
using System.Collections.Generic;
using Terraria.UI;

namespace QuiteEnoughRecipes.ModRecipes;

public class WandsRecipe : IRecipe
{
	public int WandItem { get; init; }
	public FlexibleTileWand_OptionBucket WandOptions { get; init; }


	public WandsRecipe(int wandItem, FlexibleTileWand_OptionBucket wandOptions)
	{
		WandItem = wandItem;
		WandOptions = wandOptions;
	}

	public UIElement Element => new UIWandsRecipePanel(WandItem, WandOptions);

	public IEnumerable<IIngredient> GetIngredients()
	{
		yield return new ItemIngredient(new(WandItem));

		yield return new ItemIngredient(new(WandOptions.ItemTypeToConsume));
		foreach (var option in WandOptions.Options)
		{
			yield return new TileIngredient(option.TileIdToPlace, option.TileStyleToPlace);
		}
	}
}
