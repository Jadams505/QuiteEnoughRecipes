using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace QuiteEnoughRecipes.ModAPI;

public class FilterHandlerLoader
{
	public static int FilterHandlerCount => _filterHandlers.Count;
	private static readonly List<QERFilterHandler> _filterHandlers = [];
	private static QERFilterHandler[] _orderedFilterHandlers = _filterHandlers.ToArray();
	public static IReadOnlyList<QERFilterHandler> OrderedFilterHandlers => _orderedFilterHandlers;

	internal static int Add(QERFilterHandler handler)
	{
		_filterHandlers.Add(handler);
		return FilterHandlerCount - 1;
	}

	internal static void ResizeArrays()
	{
		int slots = 1;
		var sortingSlots = new List<QERFilterHandler>[slots];
		for (int i = 0; i < sortingSlots.Length; ++i)
			sortingSlots[i] = [];

		sortingSlots[0].AddRange(_filterHandlers);

		List<QERFilterHandler> sortedLayers = [];

		for (int i = 0; i < slots; ++i)
		{
			var elements = sortingSlots[i];
			var sort = new TopoSort<QERFilterHandler>(elements,
				i => i.GetConstraints()?.OfType<After<QERFilterHandler>>().Select(a => a.Value).Where(elements.Contains) ?? [],
				i => i.GetConstraints()?.OfType<Before<QERFilterHandler>>().Select(a => a.Value).Where(elements.Contains) ?? []);

			foreach (QERFilterHandler icon in sort.Sort())
			{
				sortedLayers.Add(icon);
			}
		}

		_orderedFilterHandlers = sortedLayers.ToArray();
	}
}