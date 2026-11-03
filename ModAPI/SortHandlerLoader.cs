using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace QuiteEnoughRecipes.ModAPI;

public static class SortHandlerLoader
{
	public static int SortHandlerCount => _sortHandlers.Count;
	private static readonly List<QERSortHandler> _sortHandlers = [];
	private static QERSortHandler[] _orderedSortHandlers = _sortHandlers.ToArray();
	public static IReadOnlyList<QERSortHandler> OrderedSortHandler => _orderedSortHandlers;

	internal static int Add(QERSortHandler handler)
	{
		_sortHandlers.Add(handler);
		return SortHandlerCount - 1;
	}

	internal static void ResizeArrays()
	{
		int slots = 1;
		var sortingSlots = new List<QERSortHandler>[slots];
		for (int i = 0; i < sortingSlots.Length; ++i)
			sortingSlots[i] = [];

		sortingSlots[0].AddRange(_sortHandlers);

		List<QERSortHandler> sortedLayers = [];

		for (int i = 0; i < slots; ++i)
		{
			var elements = sortingSlots[i];
			var sort = new TopoSort<QERSortHandler>(elements,
				i => i.GetConstraints()?.OfType<QERSortHandler.After>().Select(a => a.Value).Where(elements.Contains) ?? [],
				i => i.GetConstraints()?.OfType<QERSortHandler.Before>().Select(a => a.Value).Where(elements.Contains) ?? []);

			foreach (QERSortHandler icon in sort.Sort())
			{
				sortedLayers.Add(icon);
			}
		}

		_orderedSortHandlers = sortedLayers.ToArray();
	}

	public static UISortGroup<IIngredient> GetSortGroup()
	{
		var group = new UISortGroup<IIngredient>();
		foreach (var handler in OrderedSortHandler)
		{
			group.AddSorts(handler);
		}

		return group;
	}
}