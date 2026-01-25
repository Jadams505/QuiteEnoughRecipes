using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ObjectData;

namespace QuiteEnoughRecipes.ModRecipeHandlers;

public class ChestLootHandler : IRecipeHandler
{
	private static Dictionary<int, List<DropRateInfo>> _chestLootCache = null!;
	internal static Dictionary<int, List<DropRateInfo>> ChestLootCache
	{
		get {
			return GetChestLootRates();
		}
	}

	public LocalizedText HoverName { get; }
		= Language.GetText("Mods.QuiteEnoughRecipes.Tabs.ChestLoot");

	public Item TabItem { get; } = new(ItemID.GoldChest);

	public IEnumerable<Type> GetIngredientTypes() => [typeof(ItemIngredient)];

	public IEnumerable<IRecipe> GetRecipes(IIngredient ing, QueryType queryType)
	{
		return (ing, queryType) switch
		{
			(ItemIngredient itemIng, QueryType.Sources) => ChestLootCache
				.Where(entry => entry.Value.Any(drop => drop.itemId == itemIng.Item.type))
				.Select(entry => new ItemDropsRecipe()
				{
					Item = new(entry.Key),
					Drops = entry.Value
				}),
			_ => []
		};
	}

	public static Dictionary<int, List<IEnumerable<Item>>> GetChestLootLookup()
	{
		var chestLookup = new Dictionary<int, List<IEnumerable<Item>>>();

		foreach (var chest in Main.chest)
		{
			if (chest is null || !chest.item.Any(i => i.type != 0))
				continue;

			var tile = Framing.GetTileSafely(chest.x, chest.y);
			var style = TileObjectData.GetTileStyle(tile);

			var item = ContentSamples.ItemsByType.Values.FirstOrDefault(i => i.createTile == tile.TileType && i.placeStyle == style, null);
			int itemId = ItemID.GoldenKey;
			if (item is not null)
				itemId = item.type;

			var loot = chest.item.Where(i => i.type != 0);
			if (chestLookup.TryGetValue(itemId, out var items))
			{
				chestLookup[itemId].Add(loot);
			}
			else
			{
				chestLookup[itemId] = [loot];
			}
		}

		return chestLookup;
	}

	public static Dictionary<int, List<DropRateInfo>> GetChestLootRates()
	{
		var lookup = GetChestLootLookup();
		var chestLootRates = new Dictionary<int, List<DropRateInfo>>();

		foreach (var chestLoot in lookup)
		{
			var lootInfo = new List<DropRateInfo>();
			int inventoryCount = chestLoot.Value.Count;
			foreach (var group in chestLoot.Value.SelectMany(i => i).GroupBy(x => x.type))
			{
				int minStack = 1;
				int maxStack = 1;
				int count = 0;
				int type = 0;
				foreach (var item in group)
				{
					count++;
					type = item.type;
					if (item.stack < minStack)
						minStack = item.stack;
					if (item.stack > maxStack)
						maxStack = item.stack;
				}
				lootInfo.Add(new DropRateInfo(type, minStack, maxStack, (float)count / inventoryCount));
			}
			chestLootRates[chestLoot.Key] = lootInfo;
		}
		return chestLootRates;
	}
}
