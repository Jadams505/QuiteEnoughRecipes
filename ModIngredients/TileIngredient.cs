using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace QuiteEnoughRecipes.ModIngredients;

public record struct TileIngredient(int TileType, int TileStyle = 0) : IIngredient
{
	private string _name = null;
	public string Name => _name ??= GetName();
	public Mod? Mod => TileLoader.GetTile(TileType)?.Mod;
	public IEnumerable<string> GetTooltipLines()
	{
		yield return $"Style: {TileStyle}";
	}

	public bool IsEquivalent(IIngredient other)
	{
		return other is TileIngredient tOther && 
			tOther.TileType == TileType && 
			tOther.TileStyle == TileStyle;
	}

	private readonly string GetName()
	{
		var rawName = TileID.Search.GetName(TileType);
		var mTile = TileLoader.GetTile(TileType);
		if (mTile is not null)
		{
			rawName = rawName.Replace(mTile.Mod.Name + "/", "");
		}
		return Regex.Replace(rawName, "([A-Z])", " $1").Trim();
	}
}
