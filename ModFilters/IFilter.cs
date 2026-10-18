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

public interface IFilterTab : ITab
{
	public Predicate<object> Filter { get; }
}

public record SimpleFilterTab<T>(LocalizedText HoverName, UIElement TabElement, Predicate<T> Filter) : IFilterTab<T>, IFilterTab
{
	public SimpleFilterTab(LocalizedText HoverName, int tabItem, Predicate<T> Filter) 
		: this(HoverName, ITab.ItemIcon(new(tabItem)), Filter)
	{

	}

	Predicate<object> IFilterTab.Filter => o => Filter((T)o);
}

public interface IFilterHandler<T>
{
	// The name of the filter group that will be shown in the UI.
	public abstract LocalizedText FilterGroupText { get; }

	public bool ShouldShowInUI(T obj);

	public IEnumerable<IFilterTab<T>> GetFilters(IEnumerable<T> items);
}

public interface IFilterHandler
{
	// The name of the filter group that will be shown in the UI.
	public abstract LocalizedText FilterGroupText { get; }

	public bool ShouldShowInUI(IIngredient obj);

	public IEnumerable<IFilterTab<IIngredient>> GetFilters(IEnumerable<IIngredient> items);
}