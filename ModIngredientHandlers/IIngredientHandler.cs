using System;
using System.Collections.Generic;
using System.Text;
using Terraria.Localization;

namespace QuiteEnoughRecipes.ModIngredientHandlers;

public interface IIngredientHandler
{
	// The text that will be shown when the help icon is hovered
	public LocalizedText SearchHelp { get; }

	/*
	 * Returns a queryable list of ingredients that should be displayed in a tab
	 */
	public IQueryableSource<IIngredient> GetIngredients();
}
