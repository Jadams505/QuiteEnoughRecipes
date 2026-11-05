using System.Collections.Generic;
using Terraria.Localization;

namespace QuiteEnoughRecipes.ModIngredientHandlers;

public interface IIngredientHandler : ITab
{
	// The text that will be shown when the help icon is hovered
	public LocalizedText SearchHelp { get; }

	public GridOptions Options { get; }

	/*
	 * Returns a queryable list of ingredients that should be displayed in a tab
	 */
	public IEnumerable<IGridElement<IIngredient>> GetIngredients();
}