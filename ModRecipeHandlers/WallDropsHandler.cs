using QuiteEnoughRecipes.ModIngredients;
using QuiteEnoughRecipes.ModRecipes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace QuiteEnoughRecipes.ModRecipeHandlers;

public class WallDropsHandler : IRecipeHandler
{
	public LocalizedText HoverName { get; }
		= Language.GetText("Mods.QuiteEnoughRecipes.Tabs.WallDrops");

	public Item TabItem { get; } = new(ItemID.TinHammer);

	public IEnumerable<Type> GetIngredientTypes() =>
		[typeof(WallIngredient), typeof(ItemIngredient)];

	public IEnumerable<IRecipe> GetRecipes(IIngredient ing, QueryType queryType)
	{
		return (ing, queryType) switch
		{
			(WallIngredient wallIng, QueryType.Uses) when 
			WallDropsHelper.WallTypeToItemType.TryGetValue(wallIng.WallType, out var item) => [
				new WallDropsRecipe()
				{
					WallType = wallIng.WallType,
					Drops = [new DropRateInfo(item, 1, 1, 1)]
				}],
			(ItemIngredient itemIng, QueryType.Sources) => WallDropsHelper.GetWallsThatDropItem(itemIng.Item.type)
				.Select(wall => new WallDropsRecipe() 
				{ 
					WallType = wall,
					Drops = [new DropRateInfo(itemIng.Item.type, 1, 1, 1)]
				}),
			_ => []
		};
	}
}

public static class WallDropsHelper
{
	internal static FieldInfo? WallLoader_wallTypeToItemType = typeof(WallLoader).GetField("wallTypeToItemType", BindingFlags.Static | BindingFlags.NonPublic);
	internal static Dictionary<int, int> WallTypeToItemType => WallLoader_wallTypeToItemType.GetValue(null) as Dictionary<int, int>;

	[UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "KillWall_GetItemDrops")]
	extern static int WorldGen_KillWall_GetItemDrops(WorldGen self, Tile tile);

	private static Dictionary<int, int> VanillaWallDrops()
	{
		Dictionary<int, int> lookup = [];
		var tempTile = new Tile();
		var wallIdCache = tempTile.WallType;
		for (int i = 0; i < WallID.Count; i++)
		{
			tempTile.Get<WallTypeData>().Type = (ushort)i;
			// Vanilla only checks WallType when determining the drop
			var drop = WorldGen_KillWall_GetItemDrops(null!, tempTile);
			if (drop != 0)
			{
				lookup[i] = drop;
			}
		}
		// Tiles are heavily manged by TML so resetting it to it's initial state is important
		tempTile.Get<WallTypeData>().Type = wallIdCache;
		return lookup;
	}

	private static Dictionary<int, List<int>>? _itemTypeToWallType = null;
	internal static Dictionary<int, List<int>> ItemTypeToWallType => _itemTypeToWallType ??= WallTypeToItemType
			.Where(entry => WallLoader.GetWall(entry.Value) != null && !LoaderUtils.HasOverride(WallLoader.GetWall(entry.Value), m => m.Drop))
			.Concat(VanillaWallDrops())
			.GroupBy(entry => entry.Value)
			.ToDictionary(group => group.Key, group => group.Select(entry => entry.Key).ToList());

	public static List<int> GetWallsThatDropItem(int itemId)
	{
		if (ItemTypeToWallType.TryGetValue(itemId, out var walls))
		{
			return walls;
		}
		return [];
	}
}
