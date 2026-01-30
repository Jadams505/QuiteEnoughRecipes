using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace QuiteEnoughRecipes.ModRecipeHandlers;

public class ChestLootHandler : IRecipeHandler
{
	private static Dictionary<int, List<DropRateInfo>> _chestLootCache = null!;
	internal static Dictionary<int, List<DropRateInfo>> ChestLootCache
	{
		get {
			return GetChestLootRates(Main.chest);
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

	/// <summary>
	/// Builds a lookup table of chest loot, grouped by the item type that represents each chest.
	/// </summary>
	/// <returns>
	/// A dictionary keyed by chest item type, where each value is a list of loot inventories.
	/// Each inventory is the set of non-null, non-empty items found in a single chest of that type.
	/// </returns>
	public static Dictionary<int, List<IEnumerable<Item>>> GetChestLootLookup(Chest[] chests)
	{
		var chestLookup = new Dictionary<int, List<IEnumerable<Item>>>();

		foreach (var chest in chests)
		{
			if (chest is null || !chest.item.Any(i => i is not null && i.type != 0))
				continue;

			int chestItem = ChestToItem(chest);

			var loot = chest.item.Where(i => i is not null && i.type != 0);
			if (chestLookup.TryGetValue(chestItem, out var items))
			{
				chestLookup[chestItem].Add(loot);
			}
			else
			{
				chestLookup[chestItem] = [loot];
			}
		}

		return chestLookup;
	}

	/// <summary>
	/// Gets the matching item for a particular chest. It does this by using the item that places the
	/// chest. This does not work for all chests as some chests cannot be placed (locked chests). An 
	/// alternative to look into is Chest.chestTypeToIcon2 and similar.
	/// Defaults to the Golden Key.
	/// </summary>
	public static int ChestToItem(Chest chest)
	{
		var tile = Framing.GetTileSafely(chest.x, chest.y);
		var style = TileObjectData.GetTileStyle(tile);

		var item = ContentSamples.ItemsByType.Values.FirstOrDefault(i => i.createTile == tile.TileType && i.placeStyle == style, null);

		if (item is not null)
			return item.type;

		return ItemID.GoldenKey;
	}

	/// <summary>
	/// Calculates drop rate statistics for chest loot, grouped by chest item type.
	/// </summary>
	/// <returns></returns>
	public static Dictionary<int, List<DropRateInfo>> GetChestLootRates(Chest[] chests)
	{
		var lookup = GetChestLootLookup(chests);
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

	// TODO: Vanilla has a packet that can be sent from the Client to the Server to update the
	// chest contents at a particular tile. The issues is that every tile needs to be checked since the
	// client does no know if a particular tile has a chest attached to it. (A chest is a multitile, but the actual Chest data is typically stored in the top left). Querying every tile in the world, is horrible for performance.
	public static void UpdateChestInfoFromServer(int range)
	{
		var player = Main.LocalPlayer;
		var pos = player.position.ToTileCoordinates();

		for (int x = pos.X - range; x < pos.X + range; ++x)
		{
			for (int y = pos.Y - range; y < pos.Y + range; ++y)
			{
				NetMessage.SendData(MessageID.RequestChestOpen, number: x, number2: y);
			}
		}
	}
}
