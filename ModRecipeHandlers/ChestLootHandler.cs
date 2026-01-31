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

using ChestType = (int TileType, int Style, int IconItem);

namespace QuiteEnoughRecipes.ModRecipeHandlers;

public class ChestLootHandler : IRecipeHandler
{
	private static Dictionary<int, List<DropRateInfo>> _chestLootCache = null!;
	internal static Dictionary<ChestType, List<DropRateInfo>> ChestLootCache
	{
		get {
			return GetChestLootRates(Main.chest);
		}
	}

	public LocalizedText HoverName { get; }
		= Language.GetText("Mods.QuiteEnoughRecipes.Tabs.ChestLoot");

	public Item TabItem { get; } = new(ItemID.GoldChest);

	public IEnumerable<Type> GetIngredientTypes() => [typeof(ItemIngredient)];

	public IEnumerable<IRecipe> GetRecipes(IIngredient ing, QueryType queryType) => (ing, queryType) switch
	{
		(ItemIngredient itemIng, QueryType.Sources) => ChestLootCache
			.Where(entry => entry.Value.Any(drop => drop.itemId == itemIng.Item.type))
			.Select(entry => new ItemDropsRecipe()
			{
				Item = new(entry.Key.IconItem),
				Drops = entry.Value
			}),
		(ItemIngredient itemIng, QueryType.Uses) => ChestLootCache
			.Where(entry => entry.Key.IconItem == itemIng.Item.type)
			.Select(entry => new ItemDropsRecipe()
			{
				Item = new(entry.Key.IconItem),
				Drops = entry.Value
			}),
		_ => []
	};

	/// <summary>
	/// Builds a lookup table of chest loot, grouped by the chest type that represents each chest.
	/// </summary>
	/// <returns>
	/// A dictionary keyed by chest type, where each value is a list of loot inventories.
	/// Each inventory is the set of non-null, non-empty items found in a single chest of that type.
	/// </returns>
	public static Dictionary<ChestType, List<IEnumerable<Item>>> GetChestLootLookup(Chest[] chests)
	{
		var chestLookup = new Dictionary<ChestType, List<IEnumerable<Item>>>();

		foreach (var chest in chests)
		{
			if (chest is null || !chest.item.Any(i => i is not null && i.type != 0))
				continue;

			var chestType = ChestToChestType(chest);

			var loot = chest.item.Where(i => i is not null && i.type != 0);
			if (chestLookup.TryGetValue(chestType, out var items))
			{
				chestLookup[chestType].Add(loot);
			}
			else
			{
				chestLookup[chestType] = [loot];
			}
		}

		return chestLookup;
	}

	public static ChestType ChestToChestType(Chest chest)
	{
		var tile = Framing.GetTileSafely(chest.x, chest.y);
		var style = 0;
		int itemType = 0;
        if (Main.tileFrameImportant[tile.TileType])
        {
			style = TileObjectData.GetTileStyle(tile);
		}

		var item = ContentSamples.ItemsByType.Values.FirstOrDefault(i => i.createTile == tile.TileType && i.placeStyle == style, null);
		if (item is not null)
			itemType = item.type;

		return (tile.TileType, style, itemType);
	}

	/// <summary>
	/// Calculates drop rate statistics for chest loot, grouped by chest type.
	/// </summary>
	/// <returns></returns>
	public static Dictionary<ChestType, List<DropRateInfo>> GetChestLootRates(Chest[] chests)
	{
		var lookup = GetChestLootLookup(chests);
		var chestLootRates = new Dictionary<ChestType, List<DropRateInfo>>();

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
