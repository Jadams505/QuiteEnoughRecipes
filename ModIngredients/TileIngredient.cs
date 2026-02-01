using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace QuiteEnoughRecipes.ModIngredients;

public record struct TileIngredient(int TileType, int TileStyle = 0) : IIngredient
{
	public string Name => TileID.Search.GetName(TileType);
	public Mod? Mod => TileLoader.GetTile(TileType)?.Mod;
	public IEnumerable<string> GetTooltipLines() => [];

	public bool IsEquivalent(IIngredient other)
	{
		return other is TileIngredient tOther && 
			tOther.TileType == TileType && 
			tOther.TileStyle == TileStyle;
	}
}
