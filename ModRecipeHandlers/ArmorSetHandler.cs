using QuiteEnoughRecipes.ModRecipes;
using QuiteEnoughRecipes.ModUIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace QuiteEnoughRecipes.ModRecipeHandlers;

public class ArmorSetHandler : IRecipeHandler
{
	public LocalizedText HoverName { get; }
		= Language.GetText("Mods.QuiteEnoughRecipes.Tabs.ArmorSet");
	public Item TabItem { get; } = new Item(ItemID.IronChainmail);

	public IEnumerable<Type> GetIngredientTypes() => [typeof(ItemIngredient)];

	public IEnumerable<IRecipe> GetRecipes(IIngredient ing, QueryType queryType)
	{
		if (ing is not ItemIngredient itemIng) yield break;

		if (queryType is QueryType.Uses)
		{
			
			ArmorSetBonus[] sets = ArmorSetBonuses.SetsContaining[itemIng.Item.type];
			foreach (var set in sets)
			{
				IEnumerable<int> armor = [set.Head, set.Body, set.Legs];
				yield return new ResultDropsRecipe<UIArmorSetPanel, UIItemPanel>()
				{
					Drops = armor.Where(a => a > 0).Select(a => new UIItemPanel(new(a))),
					Result = new UIArmorSetPanel(set)
				};
			}
		}
	}
}
