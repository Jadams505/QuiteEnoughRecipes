using QuiteEnoughRecipes.ModFiltersHandlers;
using System;
using System.Collections.Generic;
using System.Text;
using Terraria.Localization;
using Terraria.UI;

namespace QuiteEnoughRecipes.ModSortHandlers;

public interface ISortTab<in T> : ITab
{
	public Comparison<T> Sorter { get; }
}

public record SimpleIngredientSortTab<T> : SimpleSortTab<IIngredient>
	where T : IIngredient
{
	public SimpleIngredientSortTab(LocalizedText HoverName, Func<UIElement> TabFunc, Comparison<T> Sorter)
		: base(HoverName, TabFunc, (a, b) => a is T aT && b is T bT ? Sorter(aT, bT) : 0) // I don't think 0 is correct here
	{

	}

	public SimpleIngredientSortTab(LocalizedText HoverName, int tabItem, Comparison<T> Sorter)
		: this(HoverName, () => ITab.ItemIcon(new(tabItem)), Sorter)
	{

	}
}

public record SimpleSortTab<T>(LocalizedText HoverName, Func<UIElement> TabFunc, Comparison<T> Sorter) : ISortTab<T>
{

	public SimpleSortTab(LocalizedText HoverName, int tabItem, Comparison<T> Sorter)
		: this(HoverName, () => ITab.ItemIcon(new(tabItem)), Sorter)
	{

	}

	public UIElement TabElement => TabFunc();
}
