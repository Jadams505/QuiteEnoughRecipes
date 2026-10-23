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
		var mods = ModLoader.Mods;
		var keyParent = "Mods.QuiteEnoughRecipes.OptionGroups.Mods";

		var sortedMods = mods.Distinct().OrderBy(m => m.DisplayNameClean);
		foreach (var mod in sortedMods)
		{
			var name = Language.GetText($"{keyParent}.ModName")
				.WithFormatArgs(mod.DisplayNameClean);
			var icon = mod.ModSourceBestiaryInfoElement.GetFilterImage();
			yield return new SimpleFilterTab<IIngredient>(name, () => icon, i => i.Mod == mod);
		}
	}
}
