using QuiteEnoughRecipes.ModIngredients;
using QuiteEnoughRecipes.ModRecipes;
using QuiteEnoughRecipes.ModUIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace QuiteEnoughRecipes.ModRecipeHandlers;

public class TilesHandler : IRecipeHandler
{
	public LocalizedText HoverName { get; }
		= Language.GetText("Mods.QuiteEnoughRecipes.Tabs.PlacedTiles");

	public Item TabItem { get; } = new(ItemID.StoneBlock);

	public IEnumerable<Type> GetIngredientTypes() =>
		[typeof(TileIngredient)];

	public IEnumerable<IRecipe> GetRecipes(IIngredient ing, QueryType queryType)
	{
		return (ing, queryType) switch
		{
			(ItemIngredient itemIng, QueryType.Uses) when itemIng.Item.createTile != -1 => [new TileDropsRecipe() 
			{ 
				TileType = itemIng.Item.createTile,
				Style = itemIng.Item.placeStyle,
				Drops = [new DropRateInfo(itemIng.Item.type, 1, 1, 1)]
			}],
			(TileIngredient tileIng, QueryType.Sources) => ContentSamples.ItemsByType
				.Where(entry => entry.Value.createTile == tileIng.TileType && entry.Value.placeStyle == tileIng.TileStyle)
				.Select(entry => new TileDropsRecipe()
				{
					TileType = tileIng.TileType,
					Style = tileIng.TileStyle,
					Drops = [new DropRateInfo(entry.Value.type, 1, 1, 1)]
				}),
			_ => []
		};
	}
}
