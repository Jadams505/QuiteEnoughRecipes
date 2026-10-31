using System;
using System.Collections.Generic;
using System.Text;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace QuiteEnoughRecipes.ModAPI;

public abstract class QERRecipeHandler : ModType, IRecipeHandler
{
	public int Type { get; internal set; }
	public abstract LocalizedText HoverName { get; }
	public abstract UIElement TabElement { get; }

	public abstract IEnumerable<IRecipe> GetRecipes(IIngredient ing, QueryType queryType);

	/// <summary>
	/// By default icons are sorted in load order.<br/>
	/// This hook allows you to sort this icon before/after other icons.<br/>
	/// Example:
	/// <para>
	/// <c>yield return new After(ModContent.GetInstance&lt;RecipeHandlers.CraftingStations&gt;());</c>
	/// </para>
	/// By default, this hook returns <c>[]</c>, which indicates that this icon has no ordering constraints.
	/// </summary>
	public virtual IEnumerable<Position<QERRecipeHandler>> GetConstraints() => [];

	protected sealed override void Register()
	{
		ModTypeLookup<QERRecipeHandler>.Register(this);
		Type = RecipeHandlerLoader.Add(this);
	}

	public sealed override void SetupContent() => SetStaticDefaults();
}
