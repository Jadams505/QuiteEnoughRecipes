using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.Bestiary;
using Terraria.Localization;
using Terraria.ModLoader;

namespace QuiteEnoughRecipes.ModIngredients;
public record struct BiomeIngredient(IFilterInfoProvider InfoType, Mod? Mod = null) : IIngredient
{
	public string Name { get; } = Language.GetTextValue(InfoType.GetDisplayNameKey());

	public IEnumerable<string> GetTooltipLines()
	{
		return [];
	}

	public bool IsEquivalent(IIngredient other)
	{
		return other is BiomeIngredient bOther &&
			other.Name == other.Name;
	}
}
