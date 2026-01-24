using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;

namespace QuiteEnoughRecipes.ModRecipeHandlers;

public class ChlorophyteExtractinatorHandler : IRecipeHandler
{
	[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_options")]
	extern static ref List<ItemTrader.TradeOption> ItemTrader_options(ItemTrader self);

	public LocalizedText HoverName { get; }
		= Language.GetText("Mods.QuiteEnoughRecipes.Tabs.ChlorophyteExtractinator");

	public Item TabItem { get; } = new(ItemID.ChlorophyteExtractinator);

	public IEnumerable<Type> GetIngredientTypes() => [typeof(ItemIngredient)];

	public IEnumerable<IRecipe> GetRecipes(IIngredient ing, QueryType queryType)
	{
		if (ing is not ItemIngredient item) 
			yield break;

		var options = ItemTrader_options(ItemTrader.ChlorophyteExtractinator);

		foreach (var option in options)
		{
			if ((option.GivingITemType == item.Item.type && queryType is QueryType.Sources) ||
				(option.TakingItemType == item.Item.type && queryType is QueryType.Uses) ||
				(item.Item.type == ItemID.ChlorophyteExtractinator && queryType is QueryType.Uses))
			{
				var (taking, giving) = ChlorophyteExtracinatorTrade(option);
				yield return new BasicRecipe
				{
					Result = giving,
					RequiredItems = [taking],
					RequiredTiles = [TileID.ChlorophyteExtractinator]
				};
			}
		}
	}

	/*
	 * Gets the input and output items for a given Chlorophyte Extracinator trade. Note that
	 * the stack information is retained despite always being 1 in Vanilla.
	 */
	private static (Item taking, Item giving) ChlorophyteExtracinatorTrade(ItemTrader.TradeOption option)
	{
		var input = new Item(option.TakingItemType, option.TakingItemStack);
		var output = new Item(option.GivingITemType, option.GivingItemStack);
		return (input, output);
	}
}
