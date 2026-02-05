using QuiteEnoughRecipes.ModIngredients;
using QuiteEnoughRecipes.ModUIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.UI;

namespace QuiteEnoughRecipes.ModRecipes;

public class TileDropsRecipe : IRecipe
{
	public required int TileType { get; set; }
	public required List<DropRateInfo> Drops { get; set; }
	private int _style;
	public int Style 
	{ 
		get => _style;
		// styles should not be negative
		set => _style = Math.Max(0, value); 
	}

	public UIElement Element => new UIDropsPanel(new UITilePanel(TileType, Style, 70) { Border = 32 }, Drops);

	public IEnumerable<IIngredient> GetIngredients()
	{
		IEnumerable<IIngredient> item = [new TileIngredient(TileType, Style)];
		var drops = Drops.Select(d => new ItemIngredient(new(d.itemId)) as IIngredient);
		return item.Concat(drops);
	}
}
