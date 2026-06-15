using QuiteEnoughRecipes.ModIngredients;
using QuiteEnoughRecipes.ModRecipes;
using QuiteEnoughRecipes.ModUIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;

namespace QuiteEnoughRecipes.ModRecipeHandlers;
public class BuffingHandler : IRecipeHandler
{
	public LocalizedText HoverName { get; } =
		Language.GetText("Mods.QuiteEnoughRecipes.Tabs.Buffing");
	public Item TabItem { get; } = new(ItemID.ShinePotion);

	public IEnumerable<Type> GetIngredientTypes() => [typeof(BuffIngredient), typeof(ItemIngredient)];

	public IEnumerable<IRecipe> GetRecipes(IIngredient ing, QueryType queryType) => (ing, queryType) switch
	{
		(BuffIngredient buffIng, QueryType.Sources) => ContentSamples.ItemsByType
			.Where(i => i.Value.buffType == buffIng.BuffType)
			.Select(entry => new ResultDropsRecipe<UIItemPanel, UIBuffPanel> {
				Result = new UIItemPanel(new(entry.Key), 52),
				Drops = [new UIBuffPanel(buffIng.BuffType, 30) { Border = 10 }]
			}),
		(ItemIngredient itemIng, QueryType.Uses) when itemIng.Item.buffType > 0 => [
			new ResultDropsRecipe<UIItemPanel, UIBuffPanel> {
				Result = new UIItemPanel(new(itemIng.Item.type), 52),
				Drops = [new UIBuffPanel(itemIng.Item.buffType, 30) { Border = 10}]
			}],
		_ => []
	};
}
