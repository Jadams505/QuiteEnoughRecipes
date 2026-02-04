using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace QuiteEnoughRecipes.ModIngredients;

public record struct WallIngredient(int WallType) : IIngredient
{
	public string Name => TileID.Search.GetName(WallType);
	public Mod? Mod => WallLoader.GetWall(WallType)?.Mod;
	public IEnumerable<string> GetTooltipLines() => [];

	public bool IsEquivalent(IIngredient other)
	{
		return other is WallIngredient wOther &&
			wOther.WallType == WallType;
	}
}
