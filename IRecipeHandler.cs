using System.Collections.Generic;
using System;
using Terraria.Localization;
using Terraria;

namespace QuiteEnoughRecipes;

/*
 * Represents a tab in the recipe panel. This can be used for sources, uses, or both; the
 * distinction is made by the `QueryType` passed to `GetRecipeDisplays`.
 */
public interface IRecipeHandler : ITab
{
	/*
	 * Given an ingredient, returns a sequence of UI elements showing recipes that either use or
	 * result in that ingredient.
	 */
	public IEnumerable<IRecipe> GetRecipes(IIngredient ing, QueryType queryType);
}

public enum QueryType
{
	Sources,
	Uses
}
