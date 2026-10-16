using QuiteEnoughRecipes.ModIngredients;
using QuiteEnoughRecipes.ModRecipes;
using QuiteEnoughRecipes.ModUIElements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.GameContent.FlexibleTileWand;

namespace QuiteEnoughRecipes.ModRecipeHandlers;

public class FlexibleWandsHandler : IRecipeHandler
{
	public LocalizedText HoverName { get; }
		= Language.GetText("Mods.QuiteEnoughRecipes.Tabs.Wands");

	public Item TabItem { get; } = new Item(ItemID.LeafWand);

	public IEnumerable<Type> GetIngredientTypes() => [typeof(ItemIngredient)];

	public IEnumerable<IRecipe> GetRecipes(IIngredient ing, QueryType queryType)
	{
		if (ing is ItemIngredient itemIng)
		{
			if (queryType is QueryType.Uses)
			{
				// Include all the wand items themselves
				if (FlexibleTileWandSystem.Instance.AllWands.TryGetValue(itemIng.Item.type, out var wand))
				{
					var options = wand.Options();
					foreach (var option in options)
					{
						yield return option.Value.ToWandRecipe(itemIng.Item.type);
					}
					yield break;
				}

				// Include all the items the wand consumes
				foreach (var wandPair in FlexibleTileWandSystem.Instance.AllWands)
				{
					var wandItem = wandPair.Key;
					var allOptionsWithItem = wandPair.Value.Options().Values.Where(options => options.ContainsItem(itemIng.Item.type));
					foreach (var options in allOptionsWithItem)
					{
						yield return options.ToWandRecipe(wandItem);
					}
				}
				
			}
		}

		if (ing is TileIngredient tileIng)
		{
			if (queryType is QueryType.Sources)
			{
				foreach (var wand in FlexibleTileWandSystem.Instance.AllWands)
				{
					var wandItem = wand.Key;
					var options = wand.Value.Options();
					foreach (var option in options.Values)
					{
						if (option.ContainsTile(tileIng))
						{
							yield return option.ToWandRecipe(wandItem);
						}
					}
				}
			}
		}
	}
}

public static class FlexibleTileWandExtensions
{
	public static FieldInfo? FlexibleTileWand_optionsFieldInfo = typeof(FlexibleTileWand).GetField("_options", BindingFlags.NonPublic | BindingFlags.Instance);

	public static Type? FlexibleTileWand_OptionBucketType = typeof(FlexibleTileWand).GetNestedType("OptionBucket",
			   BindingFlags.NonPublic | BindingFlags.Instance);

	public static FieldInfo? FlexibleTileWand_OptionBucket_ItemTypeToConsume = FlexibleTileWand_OptionBucketType?.GetField("ItemTypeToConsume", BindingFlags.Public | BindingFlags.Instance);

	public static FieldInfo? FlexibleTileWand_OptionBucket_Options = FlexibleTileWand_OptionBucketType?.GetField("Options", BindingFlags.Public | BindingFlags.Instance);

	extension(FlexibleTileWand wand)
	{
		public Dictionary<int, FlexibleTileWand_OptionBucket> Options()
		{
			if (FlexibleTileWand_optionsFieldInfo is null)
				return [];
			if (FlexibleTileWand_OptionBucket_ItemTypeToConsume is null)
				return [];
			if (FlexibleTileWand_OptionBucket_Options is null)
				return [];

			var dictionary = FlexibleTileWand_optionsFieldInfo.GetValue(wand) as IDictionary;
			var typedDictionary = new Dictionary<int, FlexibleTileWand_OptionBucket>(dictionary.Count);
			foreach (var entry in dictionary)
			{
				DictionaryEntry kvp = (DictionaryEntry)entry;
				int key = (int)kvp.Key;
				var option = kvp.Value;
				int itemType = (int)FlexibleTileWand_OptionBucket_ItemTypeToConsume.GetValue(option);
				var placementOptions = FlexibleTileWand_OptionBucket_Options.GetValue(option) as List<PlacementOption>;
				var wrapper = new FlexibleTileWand_OptionBucket
				{
					ItemTypeToConsume = itemType,
					Options = placementOptions ?? []
				};
				typedDictionary[key] = wrapper;
			}

			return typedDictionary;
		}
	}

	extension(FlexibleTileWand_OptionBucket optionBucket)
	{
		public ResultDropsRecipe<UIItemPanel, UITilePanel> ToResultDropsRecipe()
		{
			return new ResultDropsRecipe<UIItemPanel, UITilePanel>()
			{
				Result = new UIItemPanel(new(optionBucket.ItemTypeToConsume), width: 70),
				Drops = optionBucket.Options.Select(e => new UITilePanel(e.TileIdToPlace, e.TileStyleToPlace, size: 50))
			};
		}

		public WandsRecipe ToWandRecipe(int wandItem) => new WandsRecipe(wandItem, optionBucket);

		public bool ContainsItem(int item)
		{
			return optionBucket.ItemTypeToConsume == item;
		}

		public bool ContainsTile(TileIngredient tile)
		{
			return optionBucket.Options.Any(option => option.TileIdToPlace == tile.TileType && option.TileStyleToPlace == tile.TileStyle);
		}
	}
}

public class FlexibleTileWand_OptionBucket
{
	public required int ItemTypeToConsume { get; init; }
	public required List<PlacementOption> Options { get; init; } = [];
}

public class FlexibleTileWandSystem : ModSystem
{
	public static FlexibleTileWandSystem Instance => field ??= ModContent.GetInstance<FlexibleTileWandSystem>();
	public Dictionary<int, FlexibleTileWand> AllWands => field ??= ContentSamples.ItemsByType
		.Where(entry => entry.Value.GetFlexibleTileWand() is not null)
		.ToDictionary(
			keySelector: entry => entry.Key, 
			elementSelector: entry => entry.Value.GetFlexibleTileWand()
		);
}
