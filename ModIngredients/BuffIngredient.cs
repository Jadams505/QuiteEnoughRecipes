using System.Collections.Generic;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace QuiteEnoughRecipes.ModIngredients;
public record struct BuffIngredient(int BuffType) : IIngredient
{
	public string Name => Lang.GetBuffName(BuffType);
	public Mod? Mod => BuffLoader.GetBuff(BuffType)?.Mod;
	public IEnumerable<string> GetTooltipLines()
	{
		yield return Lang.GetBuffDescription(BuffType);
	}

	public bool IsEquivalent(IIngredient other)
	{
		return other is BuffIngredient bOther &&
			bOther.BuffType == BuffType;
	}
}
