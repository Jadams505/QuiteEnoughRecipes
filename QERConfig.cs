using System.ComponentModel;
using Terraria.ModLoader.Config;
using Terraria.ModLoader;

namespace QuiteEnoughRecipes;

public class QERConfig : ModConfig
{
	public static QERConfig Instance => ModContent.GetInstance<QERConfig>();

	public override ConfigScope Mode => ConfigScope.ClientSide;

	[DefaultValue(false)]
	public bool ShouldPreloadItems;

	[DefaultValue(false)]
	public bool AutoFocusSearchBars;

	[DefaultValue(false)]
	public bool ShowDropChancesInTooltips;

	[DefaultValue(false)]
	public bool HighlightClickedItems;

	public bool ShouldHighlightMatchingIngredients(IIngredient? first, IIngredient? second)
	{
		if (!HighlightClickedItems) { return false; }
		if (first == null || second == null) { return false; }

		return first.IsEquivalent(second);
	}
}
