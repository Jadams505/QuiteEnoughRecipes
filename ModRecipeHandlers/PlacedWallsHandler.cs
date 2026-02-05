using QuiteEnoughRecipes.ModIngredients;
using QuiteEnoughRecipes.ModRecipes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;

namespace QuiteEnoughRecipes.ModRecipeHandlers;

public class PlacedWallsHandler : IRecipeHandler
{
	public LocalizedText HoverName { get; }
		= Language.GetText("Mods.QuiteEnoughRecipes.Tabs.PlacedWalls");

	public Item TabItem { get; } = new(ItemID.StoneWall);

	public IEnumerable<Type> GetIngredientTypes() =>
		[typeof(WallIngredient)];

	public IEnumerable<IRecipe> GetRecipes(IIngredient ing, QueryType queryType)
	{
		return (ing, queryType) switch
		{
			(ItemIngredient itemIng, QueryType.Uses) when itemIng.Item.createWall != -1 => [new WallDropsRecipe()
			{
				WallType = itemIng.Item.createWall,
				Drops = [new DropRateInfo(itemIng.Item.type, 1, 1, 1)]
			}],
			(WallIngredient wallIng, QueryType.Sources) => ContentSamples.ItemsByType
				.Where(entry => entry.Value.createWall == wallIng.WallType)
				.Select(entry => new WallDropsRecipe()
				{
					WallType = wallIng.WallType,
					Drops = [new DropRateInfo(entry.Value.type, 1, 1, 1)]
				}),
			_ => []
		};
	}
}
