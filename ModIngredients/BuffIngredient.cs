using System.Collections.Generic;
using System.Text.RegularExpressions;
using Terraria.ID;
using Terraria.ModLoader;

namespace QuiteEnoughRecipes.ModIngredients;
public record struct BuffIngredient(int BuffType) : IIngredient
{
	private string? _name = null;
	public string Name => _name ??= GetName();
	public Mod? Mod => BuffLoader.GetBuff(BuffType)?.Mod;
	public IEnumerable<string> GetTooltipLines() => [];

	public bool IsEquivalent(IIngredient other)
	{
		return other is BuffIngredient bOther &&
			bOther.BuffType == BuffType;
	}

	private readonly string GetName()
	{
		var rawName = BuffID.Search.GetName(BuffType);
		var mBuff = BuffLoader.GetBuff(BuffType);
		if (mBuff is not null)
		{
			rawName = rawName.Replace(mBuff.Mod.Name + "/", "");
		}
		return Regex.Replace(rawName, "([A-Z])", " $1").Trim();
	}
}
