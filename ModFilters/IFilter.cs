using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Terraria.Localization;
using Terraria.UI;

namespace QuiteEnoughRecipes.ModFilters;

public interface IFilterTab<in T> : ITab
{
	public Predicate<T> Filter { get; }
}

public record SimpleFilterTab<T>(LocalizedText HoverName, Func<UIElement> TabFunc, Predicate<T> Filter) : IFilterTab<T>
{
	
	public SimpleFilterTab(LocalizedText HoverName, int tabItem, Predicate<T> Filter) 
		: this(HoverName, () => ITab.ItemIcon(new(tabItem)), Filter)
	{

	}

	public UIElement TabElement => TabFunc();
}

public record SimpleIngredientFilterTab<T> : SimpleFilterTab<IIngredient>
	where T : IIngredient
{
	public SimpleIngredientFilterTab(LocalizedText HoverName, Func<UIElement> TabFunc, Predicate<T> Filter)
		: base(HoverName, TabFunc, o => o is T oIng ? Filter(oIng) : false)
	{

	}

	public SimpleIngredientFilterTab(LocalizedText HoverName, int tabItem, Predicate<T> Filter)
		: this(HoverName, () => ITab.ItemIcon(new(tabItem)), Filter)
	{

	}
}

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