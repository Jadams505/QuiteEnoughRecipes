using QuiteEnoughRecipes.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria.Localization;
using Terraria.ModLoader;

namespace QuiteEnoughRecipes.ModFiltersHandlers;

internal class ModFiltersHandler : QERFilterHandler
{
	public override LocalizedText FilterGroupText { get; }
		= Language.GetText("Mods.QuiteEnoughRecipes.OptionGroups.Mods.Name");

	public override bool Applies(IEnumerable<IIngredient> items)
	{
		return items.Any(item => item.Mod is not null);
	}

	public override IEnumerable<IFilterTab<IIngredient>> GetFilters()
	{
		var mods = IngredientHandlerLoader.OrderedIngredientHandlers
			.SelectMany(i => i.GetIngredients().Select(x => x.Value)
				.Select(x => x.Mod)
			)
			.Where(m => m is not null)
			.DistinctBy(m => m.Name)
			.OrderBy(m => m.DisplayNameClean);

		foreach (var mod in mods)
		{
			var filter = MakeModFilter(mod);
			yield return filter;
		}
	}

	private static IFilterTab<IIngredient> MakeModFilter(Mod mod)
	{
		const string keyParent = "Mods.QuiteEnoughRecipes.OptionGroups.Mods";
		var name = Language.GetText($"{keyParent}.ModName")
				.WithFormatArgs(mod.DisplayNameClean);
		var icon = mod.ModSourceBestiaryInfoElement.GetFilterImage();
		
		return new SimpleFilterTab<IIngredient>(name, () => icon, i => i.Mod == mod);
	}
}
