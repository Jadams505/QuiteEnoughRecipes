using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Terraria.GameContent.Bestiary;
using Terraria.Localization;
using Terraria.ModLoader;

namespace QuiteEnoughRecipes.ModIngredients;
public record struct BiomeIngredient(IFilterInfoProvider InfoType) : IIngredient
{
	public string Name { get; } = Language.GetTextValue(InfoType.GetDisplayNameKey());

	public readonly Mod? Mod => InfoType is ModBestiaryInfoElement moddedInfo 
		? moddedInfo.ModBestiaryInfoElement_mod() : null;

	public IEnumerable<string> GetTooltipLines()
	{
		return [];
	}

	public bool IsEquivalent(IIngredient other)
	{
		return other is BiomeIngredient bOther &&
			Name == bOther.Name;
	}
}

public static class ModBestiaryInfoElement_Extensions
{
	[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_mod")]
	internal static extern ref Mod ModBestiaryInfoElement_mod(this ModBestiaryInfoElement self);
}
