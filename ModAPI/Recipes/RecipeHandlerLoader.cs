using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace QuiteEnoughRecipes.ModAPI.Recipes;

public class RecipeHandlerLoader
{
	public static int RecipeHandlerCount => _recipeHandlers.Count;
	private static readonly List<QERRecipeHandler> _recipeHandlers = [];
	private static QERRecipeHandler[] _orderedRecipeHandlers = _recipeHandlers.ToArray();
	public static IReadOnlyList<QERRecipeHandler> OrderedRecipeHandlers => _orderedRecipeHandlers;

	internal static int Add(QERRecipeHandler handler)
	{
		_recipeHandlers.Add(handler);
		return RecipeHandlerCount - 1;
	}

	internal static void ResizeArrays()
	{
		int slots = 1;
		var sortingSlots = new List<QERRecipeHandler>[slots];
		for (int i = 0; i < sortingSlots.Length; ++i)
			sortingSlots[i] = [];

		sortingSlots[0].AddRange(_recipeHandlers);

		List<QERRecipeHandler> sortedLayers = [];

		for (int i = 0; i < slots; ++i)
		{
			var elements = sortingSlots[i];
			var sort = new TopoSort<QERRecipeHandler>(elements,
				i => i.GetConstraints()?.OfType<QERRecipeHandler.After>().Select(a => a.Value).Where(elements.Contains) ?? [],
				i => i.GetConstraints()?.OfType<QERRecipeHandler.Before>().Select(a => a.Value).Where(elements.Contains) ?? []);

			foreach (QERRecipeHandler icon in sort.Sort())
			{
				sortedLayers.Add(icon);
			}
		}

		_orderedRecipeHandlers = sortedLayers.ToArray();
	}
}