using QuiteEnoughRecipes.ModIngredients;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

using DropCountInfo = (int MinStack, int MaxStack, int Count);

namespace QuiteEnoughRecipes.ModRecipeHandlers;

public class ExtractinatorHandler : IRecipeHandler
{
	internal Dictionary<int /*Input Item*/, Dictionary<int /*Output Item*/, DropRateInfo>> ExtractinatorDropsCache
	{
		get => field ??= CreateDropsCache();
	}

	public LocalizedText HoverName { get; }
		= Language.GetText("Mods.QuiteEnoughRecipes.Tabs.Extractinator");
	public Item TabItem { get; } = new Item(ItemID.Extractinator);

	public IEnumerable<Type> GetIngredientTypes() => [typeof(ItemIngredient)];

	public IEnumerable<IRecipe> GetRecipes(IIngredient ing, QueryType queryType)
	{
		if (ing is ItemIngredient itemIng)
		{
			if (queryType is QueryType.Uses)
			{
				if (ExtractinatorDropsCache.TryGetValue(itemIng.Item.type, out var drops) && drops.Count > 0)
				{
					yield return new ItemDropsRecipe()
					{
						Drops = drops.Select(e => e.Value).ToList(),
						Item = new(itemIng.Item.type)
					};
				}

				if (itemIng.Item.type is ItemID.Extractinator or ItemID.ChlorophyteExtractinator)
				{
					foreach (var entry in ExtractinatorDropsCache)
					{
						yield return new ItemDropsRecipe()
						{
							Drops = entry.Value.Select(e => e.Value).ToList(),
							Item = new(entry.Key)
						};
					}
				}
			}

			if (queryType is QueryType.Sources)
			{
				foreach (var entry in ExtractinatorDropsCache)
				{
					if (entry.Value.ContainsKey(itemIng.Item.type))
					{
						yield return new ItemDropsRecipe()
						{
							Drops = entry.Value.Select(e => e.Value).ToList(),
							Item = new(entry.Key)
						};
					}
				}
			}
		}

		if (ing is TileIngredient tileIng)
		{
			if (tileIng.TileType is not TileID.Extractinator and not TileID.ChlorophyteExtractinator)
				yield break;
			if (queryType is not QueryType.Uses)
				yield break;

			foreach (var entry in ExtractinatorDropsCache)
			{
				yield return new ItemDropsRecipe()
				{
					Drops = entry.Value.Select(e => e.Value).ToList(),
					Item = new(entry.Key)
				};
			}
		}
	}

	internal Dictionary<int /*Input Item*/, Dictionary<int /*Output Item*/, DropRateInfo>> CreateDropsCache()
	{
		var cache = new Dictionary<int /*Input Item*/, Dictionary<int /*Output Item*/, DropRateInfo>>();
		for (int i = 0; i < ItemID.Sets.ExtractinatorMode.Length; ++i)
		{
			int mode = ItemID.Sets.ExtractinatorMode[i];
			if (mode == -1)
				continue;

			// Some of the items are very rare, even 1 million runs is not enough to caputure everything.
			// 100,000 is good enough to capture all the drops, but not the max stacks.
			// Pause is also mimimal when loading for first time
			var stats = GetItemStatistics(i, TileID.Extractinator, 100_000);
			int totalCount = stats[i].Sum(e => e.Value.Count);
			cache[i] = stats[i].ToDictionary(
				e => e.Key,
				e => new DropRateInfo(e.Key, e.Value.MinStack, e.Value.MaxStack, (float)e.Value.Count / totalCount)
			);

		}
		return cache;
	}

	internal static Dictionary<int /*Input Item*/, Dictionary<int /*Output Item*/, DropCountInfo>> GetItemStatistics(int inputItem, int extractinatorBlockType, int numberOfRuns)
	{
		var finalResult = new Dictionary<int, Dictionary<int, DropCountInfo>>();

		for (int i = 0; i < numberOfRuns; i++)
		{
			var extractionMode = ItemID.Sets.ExtractinatorMode[inputItem];
			var result = GetExtractinatorDrop(extractionMode, extractinatorBlockType);

			if (!finalResult.TryGetValue(inputItem, out Dictionary<int, DropCountInfo>? modeStats))
			{
				modeStats = new Dictionary<int, DropCountInfo>();
				finalResult[inputItem] = modeStats;
			}

			if (!modeStats.TryGetValue(result.ItemType, out DropCountInfo existingStats))
			{
				modeStats[result.ItemType] = (result.Stack, result.Stack, 1);
			}
			else
			{
				var newStats = (
					Math.Min(existingStats.MinStack, result.Stack),
					Math.Max(existingStats.MaxStack, result.Stack),
					existingStats.Count + 1
				);
				modeStats[result.ItemType] = newStats;
			}
		}

		return finalResult;
	}


	private static (int ItemType, int Stack) GetExtractinatorDrop(int extractionMode, int extractinatorBlockType)
	{
		ExtractinatorHelper.RollExtractinatorDrop(extractionMode, extractinatorBlockType, out var itemType, out var stack);

		ItemLoader.ExtractinatorUse(ref itemType, ref stack, extractionMode, extractinatorBlockType);

		return (itemType, stack);
	}
}
