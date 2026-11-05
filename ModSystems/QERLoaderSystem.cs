using QuiteEnoughRecipes.ModAPI.Filters;
using QuiteEnoughRecipes.ModAPI.Ingredients;
using QuiteEnoughRecipes.ModAPI.Recipes;
using QuiteEnoughRecipes.ModAPI.Sorts;
using Terraria.ModLoader;

namespace QuiteEnoughRecipes.ModSystems;

public class QERLoaderSystem : ModSystem
{
	public static QERLoaderSystem Instance => field ??= ModContent.GetInstance<QERLoaderSystem>();

	public override void ResizeArrays()
	{
		RecipeHandlerLoader.ResizeArrays();
		IngredientHandlerLoader.ResizeArrays();
		FilterHandlerLoader.ResizeArrays();
		SortHandlerLoader.ResizeArrays();
	}
}
