using QuiteEnoughRecipes.ModIngredients;
using QuiteEnoughRecipes.ModRecipes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace QuiteEnoughRecipes.ModRecipeHandlers;

public class TileDropsHandler : IRecipeHandler
{
	public LocalizedText HoverName { get; }
		= Language.GetText("Mods.QuiteEnoughRecipes.Tabs.TileDrops");

	public Item TabItem { get; } = new(ItemID.TinPickaxe);

	public IEnumerable<Type> GetIngredientTypes() =>
		[typeof(TileIngredient), typeof(ItemIngredient)];

	public IEnumerable<IRecipe> GetRecipes(IIngredient ing, QueryType queryType)
	{
		return (ing, queryType) switch
		{
			(TileIngredient tileIng, QueryType.Uses) when 
			TileDropsHelper.TileTypeAndTileStyleToItemType.TryGetValue((tileIng.TileType, tileIng.TileStyle), out var item) ||
			TileDropsHelper.TileTypeAndTileStyleToItemType.TryGetValue((tileIng.TileType, -1), out item) => [new TileDropsRecipe()
			{
				TileType = tileIng.TileType,
				Style = tileIng.TileStyle,
				Drops = [new DropRateInfo(item, 1, 1, 1)]
			}],
			(ItemIngredient itemIng, QueryType.Sources) => TileDropsHelper.GetTilesThatDropItem(itemIng.Item.type)
				.Select(tile => new TileDropsRecipe() 
				{ 
					TileType = tile.TileId,
					Style = tile.Style,
					Drops = [new DropRateInfo(itemIng.Item.type, 1, 1, 1)]
				}),
			_ => []
		};
	}
}

public static class TileDropsHelper
{
	internal static FieldInfo? TileLoader_tileTypeAndTileStyleToItemType = typeof(TileLoader).GetField("tileTypeAndTileStyleToItemType", BindingFlags.Static | BindingFlags.NonPublic);
	internal static Dictionary<(int TileId, int Style), int> TileTypeAndTileStyleToItemType => (Dictionary<(int, int), int>)TileLoader_tileTypeAndTileStyleToItemType?.GetValue(null)!;

	private static Dictionary<int, List<(int TileId, int Style)>>? _itemTypeToTileTypeAndTileStyle = null;
	internal static Dictionary<int, List<(int TileId, int Style)>> ItemTypeToTileTypeAndTileStyle =>
		_itemTypeToTileTypeAndTileStyle ??= TileTypeAndTileStyleToItemType.GroupBy(entry => entry.Value)
			.ToDictionary(entry => entry.Key, entry => entry.Select(entry => entry.Key).ToList());

	public static List<(int TileId, int Style)> GetTilesThatDropItem(int itemId)
	{
		if (ItemTypeToTileTypeAndTileStyle.TryGetValue(itemId, out var tile))
		{
			return tile;
		}
		return [];
	}
}
