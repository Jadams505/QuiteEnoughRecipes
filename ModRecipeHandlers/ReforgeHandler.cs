using QuiteEnoughRecipes.ModRecipes;
using QuiteEnoughRecipes.ModUIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace QuiteEnoughRecipes.ModRecipeHandlers;

public class ReforgeHandler : IRecipeHandler
{
	public LocalizedText HoverName { get; }
		= Language.GetText("Mods.QuiteEnoughRecipes.Tabs.Reforging");

	public Item TabItem { get; } = new(ItemID.IronHammer);

	public IEnumerable<Type> GetIngredientTypes() => [typeof(ItemIngredient)];

    public IEnumerable<IRecipe> GetRecipes(IIngredient ing, QueryType queryType)
    {
		if (ing is not ItemIngredient item)
			yield break;
		if (queryType is not QueryType.Uses)
			yield break;
		if (!item.Item.CanHavePrefixes())
			yield break;

		for (int i = 0; i < PrefixLoader.PrefixCount; ++i)
		{
			var reforgeCopy = new Item(item.Item.type);
			if (reforgeCopy.Prefix(i))
			{
                yield return new ReforgeRecipe(reforgeCopy);
			}
		}
    }
}
