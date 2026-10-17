using System;
using System.Collections.Generic;
using System.Text;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace QuiteEnoughRecipes.ModAPI;

public abstract class QERIngredientHandler : ModType
{
	public int Type { get; internal set; }

	// The name that will be shown when this tab is hovered.
	public abstract LocalizedText HoverName { get; }

	// The text that will be shown when the help icon is hovered
	public abstract LocalizedText SearchHelp { get; }

	// Item icon that will be displayed in the tab.
	public abstract Item TabItem { get; }

	/*
	 * Returns a queryable list of ingredients that should be displayed ia tab
	 */
	public abstract IQueryableSource<IIngredient> GetIngredients();

	/// <summary>
	/// By default icons are sorted in load order.<br/>
	/// This hook allows you to sort this icon before/after other icons.<br/>
	/// Example:
	/// <para>
	/// <c>yield return new After(ModContent.GetInstance&lt;RecipeHandlers.CraftingStations&gt;());</c>
	/// </para>
	/// By default, this hook returns <c>[]</c>, which indicates that this icon has no ordering constraints.
	/// </summary>
	public virtual IEnumerable<Position<QERIngredientHandler>> GetConstraints() => [];

	protected sealed override void Register()
	{
		ModTypeLookup<QERIngredientHandler>.Register(this);
		Type = IngredientHandlerLoader.Add(this);
	}

	public sealed override void SetupContent() => SetStaticDefaults();
}
