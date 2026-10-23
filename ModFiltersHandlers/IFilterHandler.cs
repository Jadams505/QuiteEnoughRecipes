using System;
using System.Collections.Generic;
using System.Text;
using Terraria.Localization;

namespace QuiteEnoughRecipes.ModFiltersHandlers;

public interface IFilterHandler<T>
{
	// The name of the filter group that will be shown in the UI.
	public abstract LocalizedText FilterGroupText { get; }

	/// <summary>
	/// Determines if this set of filters applies to a given list of items.
	/// </summary>
	/// <param name="items"></param>
	/// <returns></returns>
	public bool Applies(IEnumerable<T> items);

	/// <summary>
	/// Returns a list of filters for this group
	/// </summary>
	/// <returns></returns>
	public IEnumerable<IFilterTab<T>> GetFilters();
}
