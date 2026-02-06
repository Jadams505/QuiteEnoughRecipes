using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace QuiteEnoughRecipes.ModIngredients;

public record struct WallIngredient(int WallType) : IIngredient
{
	private string? _name = null;
	public string Name => _name ??= GetName();
	public Mod? Mod => WallLoader.GetWall(WallType)?.Mod;
	public IEnumerable<string> GetTooltipLines() => [];

	public bool IsEquivalent(IIngredient other)
	{
		return other is WallIngredient wOther &&
			wOther.WallType == WallType;
	}

	private readonly string GetName()
	{
		var rawName = WallID.Search.GetName(WallType);
		var mWall = WallLoader.GetWall(WallType);
		if (mWall is not null)
		{
			rawName = rawName.Replace(mWall.Mod.Name + "/", "");
		}
		return Regex.Replace(rawName, "([A-Z])", " $1").Trim();
	}
}
