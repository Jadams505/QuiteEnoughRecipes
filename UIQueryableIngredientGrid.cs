using System.Collections.Generic;
using System.Linq;
using System;
using Terraria.UI;
using Terraria.GameContent.UI.Elements;
using QuiteEnoughRecipes.ModIngredientHandlers;
using QuiteEnoughRecipes.ModAPI;

namespace QuiteEnoughRecipes;

// A wrapper around `UIScrollableGrid` that can be queried.
public class UIQueryableIngredientGrid : UIElement, IQueryable
{
	private List<IIngredient> _allIngredients;
	private List<IIngredient> _filteredIngredients;

	private string _searchText = "";
	private UIFilterGroup<IIngredient> _filterGroup;
	private UISortGroup<IIngredient> _sortGroup;

	private UIScrollableGrid<IIngredient> _grid;

	public int TotalResultCount => _allIngredients.Count;
	public int DisplayedResultCount => _filteredIngredients.Count;

	public UIQueryableIngredientGrid(IIngredientHandler handler)
	{
		const float ScrollBarWidth = 30;

		_allIngredients = handler.GetIngredients().Select(x => x.Value).ToList();
		_grid = new()
		{
			Options = handler.Options,
			TemplateElement = handler.GetIngredients().First()
		};
		_filteredIngredients = new(_allIngredients);
		_filterGroup = IngredientRegistry.Instance.GetFilterGroup();
		_sortGroup = IngredientRegistry.Instance.GetSortGroup();

		_filterGroup.OnFiltersChanged += UpdateDisplayedIngredients;
		_sortGroup.OnSortChanged += UpdateDisplayedIngredients;

		var scroll = new UIScrollbar();
		scroll.Height.Percent = 1;
		scroll.HAlign = 1;
		scroll.VAlign = 1;

		_grid.Scrollbar = scroll;
		_grid.Values = _filteredIngredients;
		_grid.Width = new StyleDimension(-ScrollBarWidth, 1);
		_grid.Height.Percent = 1;
		_grid.VAlign = 1;

		Append(_grid);
		Append(scroll);
	}

	public void SetSearchText(string text)
	{
		_searchText = text;
		UpdateDisplayedIngredients();
	}

	public IEnumerable<IOptionGroup> GetFilterGroups()
	{
		return [_filterGroup];
	}

	public IEnumerable<IOptionGroup> GetSortGroups()
	{
		return [_sortGroup];
	}

	// Update what ingredients are being displayed based on the search bar and filters.
	private void UpdateDisplayedIngredients()
	{
		foreach (var handler in FilterHandlerLoader.OrderedFilterHandlers)
		{
			bool shouldShow = handler.Applies(_allIngredients.OfType<IIngredient>());
			_filterGroup.SetSectionVisibility(handler.FilterGroupText.Key, shouldShow);
		}

		foreach (var handler in SortHandlerLoader.OrderedSortHandler)
		{
			bool shouldShow = handler.Applies(_allIngredients.OfType<IIngredient>());
			_sortGroup.SetSectionVisibility(handler.SortGroupText.Key, shouldShow);
		}
		var query = SearchQuery.FromSearchText(_searchText ?? "");
		var positiveFilters = _filterGroup.GetPositiveFilters();
		var negativeFilters = _filterGroup.GetNegativeFilters();

		var filteredIngredients = _allIngredients
			.Where(i => query.Matches(i)
				&& positiveFilters.All(f => f(i))
				&& !negativeFilters.Any(f => f(i)));

		_filteredIngredients.Clear();
		_filteredIngredients.AddRange(filteredIngredients);

		_filteredIngredients.Sort(_sortGroup.GetActiveSort());

		_grid.Values = _filteredIngredients;
	}
}
