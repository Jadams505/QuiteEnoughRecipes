using QuiteEnoughRecipes.ModFiltersHandlers;
using QuiteEnoughRecipes.ModSortHandlers;
using System.Collections.Generic;
using Terraria.Localization;
using Terraria.ModLoader;

namespace QuiteEnoughRecipes.ModAPI;

public abstract class QERSortHandler : ModType, ISortHandler<IIngredient>
{
	public int Type { get; internal set; }

	public abstract LocalizedText SortGroupText { get; }

	public abstract bool Applies(IEnumerable<IIngredient> items);

	public abstract IEnumerable<ISortTab<IIngredient>> GetSorts();

	/// <summary>
	/// By default icons are sorted in load order.<br/>
	/// This hook allows you to sort this icon before/after other icons.<br/>
	/// Example:
	/// <para>
	/// <c>yield return new After(ModContent.GetInstance&lt;RecipeHandlers.CraftingStations&gt;());</c>
	/// </para>
	/// By default, this hook returns <c>[]</c>, which indicates that this icon has no ordering constraints.
	/// </summary>
	public virtual IEnumerable<Position<QERSortHandler>> GetConstraints() => [];

	protected sealed override void Register()
	{
		ModTypeLookup<QERSortHandler>.Register(this);
		Type = SortHandlerLoader.Add(this);
	}

	public sealed override void SetupContent() => SetStaticDefaults();
}
