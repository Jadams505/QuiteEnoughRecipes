using System;
using System.Collections.Generic;
using System.Text;
using Terraria.Localization;
using Terraria.UI;

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

public interface IScollableIngredient<E>
	where E : UIElement, IScrollableGridElement<IIngredient>, new()
{
	public IIngredient Ingredient { get; }
	public E Element { get; }
}
