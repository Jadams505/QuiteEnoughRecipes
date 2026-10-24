using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria.ModLoader;

namespace QuiteEnoughRecipes.ModAPI;

public class IngredientHandlerLoader
{
	public static int IngredientHandlerCount => _ingredientHandlers.Count;
	private static readonly List<QERIngredientHandler> _ingredientHandlers = [];
	private static QERIngredientHandler[] _orderedIngredientHandlers = _ingredientHandlers.ToArray();
	public static IReadOnlyList<QERIngredientHandler> OrderedIngredientHandlers => _orderedIngredientHandlers;

	internal static int Add(QERIngredientHandler handler)
	{
		_ingredientHandlers.Add(handler);
		return IngredientHandlerCount - 1;
	}

	internal static UISearchPage CreateSearchPage(QERIngredientHandler handler)
	{
		var grid = new UIQueryableIngredientGrid(handler);
		return new UISearchPage(grid, handler.SearchHelp);
	}

	internal static void ResizeArrays()
	{
		int slots = 1;
		var sortingSlots = new List<QERIngredientHandler>[slots];
		for (int i = 0; i < sortingSlots.Length; ++i)
			sortingSlots[i] = [];

		sortingSlots[0].AddRange(_ingredientHandlers);

		List<QERIngredientHandler> sortedLayers = [];

		for (int i = 0; i < slots; ++i)
		{
			var elements = sortingSlots[i];
			var sort = new TopoSort<QERIngredientHandler>(elements,
				i => i.GetConstraints()?.OfType<After<QERIngredientHandler>>().Select(a => a.Value).Where(elements.Contains) ?? [],
				i => i.GetConstraints()?.OfType<Before<QERIngredientHandler>>().Select(a => a.Value).Where(elements.Contains) ?? []);

			foreach (QERIngredientHandler icon in sort.Sort())
			{
				sortedLayers.Add(icon);
			}
		}

		_orderedIngredientHandlers = sortedLayers.ToArray();
	}
}
