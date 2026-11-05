using System;
using Terraria.Localization;
using Terraria.UI;

namespace QuiteEnoughRecipes.ModAPI.Filters;

public interface IFilterTab<in T> : ITab
{
	public Predicate<T> Filter { get; }
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

public record SimpleFilterTab<T>(LocalizedText HoverName, Func<UIElement> TabFunc, Predicate<T> Filter) : IFilterTab<T>
{

	public SimpleFilterTab(LocalizedText HoverName, int tabItem, Predicate<T> Filter)
		: this(HoverName, () => ITab.ItemIcon(new(tabItem)), Filter)
	{

	}

	public UIElement TabElement => TabFunc();
}