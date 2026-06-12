using QuiteEnoughRecipes.ModIngredients;
using QuiteEnoughRecipes.ModUIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.ItemDropRules;
using Terraria.UI;

namespace QuiteEnoughRecipes.ModRecipes;
public class ResultDropsRecipe<TResult, TDrop> : IRecipe 
	where TResult : UIElement, IIngredientElement 
	where TDrop : UIElement, IIngredientElement
{
	public required TResult Result { get; set; }
	public required IEnumerable<TDrop> Drops { get; set; }

	public UIElement Element => new UIListDropsPanel<TDrop>(Result, Drops);

	public IEnumerable<IIngredient> GetIngredients()
	{
		if (Result.Ingredient is not null)
			yield return Result.Ingredient;

		foreach (var drop in Drops)
		{
			if (drop.Ingredient is not null)
				yield return drop.Ingredient;
		}
	}
}
