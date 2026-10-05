using QuiteEnoughRecipes.ModRecipes;
using QuiteEnoughRecipes.ModUIElements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Utilities;
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
			if (queryType is QueryType.Uses && itemIng.Item.GetFlexibleTileWand() is FlexibleTileWand wand)
			{
				var options = wand.Options();
				foreach (var option in options)
				{
					yield return new ResultDropsRecipe<UIItemPanel, UITilePanel>()
					{
						Result = new UIItemPanel(new(option.Value.ItemTypeToConsume), width: 70),
						Drops = option.Value.Options.Select(e => new UITilePanel(e.TileIdToPlace, e.TileStyleToPlace, size: 50))
					};
				}
			}
		}
		yield break;
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
			{
				return null;
			}
			var dictionary = FlexibleTileWand_optionsFieldInfo.GetValue(wand) as IDictionary;
			var typedDictionary = new Dictionary<int, FlexibleTileWand_OptionBucket>(dictionary.Count);
			foreach (var entry in dictionary)
			{
				DictionaryEntry kvp = (DictionaryEntry)entry;
				int key = (int)kvp.Key;
				var option = kvp.Value;
				var itemType = (int)FlexibleTileWand_OptionBucket_ItemTypeToConsume.GetValue(option);
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
}

public class FlexibleTileWand_OptionBucket
{
	public required int ItemTypeToConsume { get; init; }
	public required List<PlacementOption> Options { get; init; } = [];
}
