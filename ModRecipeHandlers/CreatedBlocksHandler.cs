using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.Localization;

namespace QuiteEnoughRecipes.ModRecipeHandlers;

public class CreatedBlocksHandler : IRecipeHandler
{
	public LocalizedText HoverName { get; }
		= Language.GetText("Mods.QuiteEnoughRecipes.Tabs.CreatedBlocks");

	public Item TabItem { get; } = new(ItemID.StoneBlock);

	public IEnumerable<Type> GetIngredientTypes()
	{
		throw new NotImplementedException();
	}

	public IEnumerable<IRecipe> GetRecipes(IIngredient ing, QueryType queryType)
	{
		throw new NotImplementedException();
	}
}
