using QuiteEnoughRecipes.ModAPI;
using QuiteEnoughRecipes.ModRecipes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace QuiteEnoughRecipes.ModIngredientHandlers;

public class ItemIngredientHandler : QERIngredientHandler
{
	public override LocalizedText HoverName { get; } = Language.GetText("Mods.QuiteEnoughRecipes.Tabs.ItemList");
	public override LocalizedText SearchHelp { get; } = Language.GetText("Mods.QuiteEnoughRecipes.UI.ItemSearchHelp");
	public override UIElement TabElement { get; } = ITab.ItemIcon(new Item(ItemID.IronBar));

	public override IQueryableSource<IIngredient> GetIngredients()
	{
		var allItems = Enumerable.Range(0, ItemLoader.ItemCount)
			.Select(i => new Item(i))
			.Where(i => i.type != 0)
			.Select(i => new ItemIngredient(i));

		return new UIQueryableIngredientGrid<ItemIngredient, UIItemPanel>(allItems.ToList());
	}
}