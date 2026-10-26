using QuiteEnoughRecipes.ModFiltersHandlers;
using System;
using System.Collections.Generic;
using System.Text;
using Terraria.Localization;

namespace QuiteEnoughRecipes.ModSortHandlers;

public interface ISortHandler<T>
{
	// The name of the sort group that will be shown in the UI.
	public abstract LocalizedText SortGroupText { get; }

	/// <summary>
	/// Determines if this set of sorts applies to a given list of items.
	/// </summary>
	/// <param name="items"></param>
	/// <returns></returns>
	public bool Applies(IEnumerable<T> items);

	/// <summary>
	/// Returns a list of sorts for this group
	/// </summary>
	/// <returns></returns>
	public IEnumerable<ISortTab<T>> GetSorts();
}
