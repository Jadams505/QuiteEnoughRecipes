using QuiteEnoughRecipes.ModAPI;
using QuiteEnoughRecipes.ModFiltersHandlers;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using static Terraria.ID.ContentSamples.CreativeHelper;
using static QuiteEnoughRecipes.IngredientOptions;

namespace QuiteEnoughRecipes.ModFilters;

public class MiscItemFiltersHandler : QERFilterHandler
{
	public const string GroupKey = "Mods.QuiteEnoughRecipes.OptionGroups.ItemFilters.Misc";
	public override LocalizedText FilterGroupText { get; }
		= Language.GetText($"{GroupKey}.Name");

	public override IEnumerable<IFilterTab<IIngredient>> GetFilters()
	{
		yield return new SimpleIngredientFilterTab<ItemIngredient>(Language.GetText($"{GroupKey}.IsTile"), ItemID.StoneBlock, IsTile);

		yield return new SimpleIngredientFilterTab<ItemIngredient>(Language.GetText($"{GroupKey}.IsCraftingStation"), ItemID.Furnace, IsCraftingStation);

		yield return new SimpleIngredientFilterTab<ItemIngredient>(Language.GetText($"{GroupKey}.IsBossSummon"), ItemID.SuspiciousLookingEye, IsBossSummon);

		yield return new SimpleIngredientFilterTab<ItemIngredient>(Language.GetText($"{GroupKey}.IsLootItem"), ItemID.GoodieBag, IsLootItem);

		yield return new SimpleIngredientFilterTab<ItemIngredient>(Language.GetText($"{GroupKey}.IsPotion"), ItemID.InfernoPotion, IsPotion);

		yield return new SimpleIngredientFilterTab<ItemIngredient>(Language.GetText($"{GroupKey}.IsFood"), ItemID.Apple, IsFood);

		yield return new SimpleIngredientFilterTab<ItemIngredient>(Language.GetText($"{GroupKey}.IsFishing"), ItemID.WoodFishingPole, IsFishing);

		yield return new SimpleIngredientFilterTab<ItemIngredient>(Language.GetText($"{GroupKey}.IsDye"), ItemID.RedDye, IsDye);

		yield return new SimpleIngredientFilterTab<ItemIngredient>(Language.GetText($"{GroupKey}.IsAccessory"), ItemID.AnkletoftheWind, IsAccessory);

		yield return new SimpleIngredientFilterTab<ItemIngredient>(Language.GetText($"{GroupKey}.IsPet"), ItemID.ExoticEasternChewToy, IsPet);

		yield return new SimpleIngredientFilterTab<ItemIngredient>(Language.GetText($"{GroupKey}.IsMount"), ItemID.SlimySaddle, IsMount);

		yield return new SimpleIngredientFilterTab<ItemIngredient>(Language.GetText($"{GroupKey}.IsWings"), ItemID.CreativeWings, IsWings);

		yield return new SimpleIngredientFilterTab<ItemIngredient>(Language.GetText($"{GroupKey}.IsHook"), ItemID.GrapplingHook, IsHook);

		yield return new SimpleIngredientFilterTab<ItemIngredient>(Language.GetText($"{GroupKey}.IsTool"), ItemID.CopperPickaxe, IsTool);

		yield return new SimpleIngredientFilterTab<ItemIngredient>(Language.GetText($"{GroupKey}.IsArmor"), ItemID.CopperChainmail, IsArmor);

		yield return new SimpleIngredientFilterTab<ItemIngredient>(Language.GetText($"{GroupKey}.IsVanity"), ItemID.RedHat, IsVanity);

		yield return new SimpleIngredientFilterTab<ItemIngredient>(Language.GetText($"{GroupKey}.IsResearched"), ItemID.Cog, IsResearched);
	}

	public override bool Applies(IEnumerable<IIngredient> items)
	{
		return items.Any(i => i is ItemIngredient);
	}
}
