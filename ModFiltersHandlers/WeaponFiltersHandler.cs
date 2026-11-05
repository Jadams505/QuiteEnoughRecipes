using QuiteEnoughRecipes.ModAPI.Filters;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;
using Terraria.Localization;
using static QuiteEnoughRecipes.IngredientOptions;

namespace QuiteEnoughRecipes.ModFilters;

public class WeaponFiltersHandler : QERFilterHandler
{
	public const string GroupKey = "Mods.QuiteEnoughRecipes.OptionGroups.ItemFilters.Weapons";
	public override LocalizedText FilterGroupText { get; }
		= Language.GetText($"{GroupKey}.Name");

	public override IEnumerable<IFilterTab<IIngredient>> GetFilters()
	{
		yield return new SimpleIngredientFilterTab<ItemIngredient>(Language.GetText($"{GroupKey}.IsMeleeWeapon"), ItemID.CopperShortsword, IsMeleeWeapon);

		yield return new SimpleIngredientFilterTab<ItemIngredient>(Language.GetText($"{GroupKey}.IsRangedWeapon"), ItemID.WoodenBow, IsRangedWeapon);

		yield return new SimpleIngredientFilterTab<ItemIngredient>(Language.GetText($"{GroupKey}.IsMagicWeapon"), ItemID.WandofSparking, IsMagicWeapon);

		yield return new SimpleIngredientFilterTab<ItemIngredient>(Language.GetText($"{GroupKey}.IsSummonWeapon"), ItemID.BabyBirdStaff, IsSummonWeapon);

		yield return new SimpleIngredientFilterTab<ItemIngredient>(Language.GetText($"{GroupKey}.IsClasslessWeapon"), ItemID.FlareGun, IsClasslessWeapon);

		foreach (var filter in CustomWeaponFilters(GroupKey))
		{
			yield return new SimpleIngredientFilterTab<ItemIngredient>(filter.Name, filter.IconID, filter.Value);
		}
	}

	public override bool Applies(IEnumerable<IIngredient> items)
	{
		return items.Any(i => i is ItemIngredient);
	}
}
