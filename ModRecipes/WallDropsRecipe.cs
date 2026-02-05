using QuiteEnoughRecipes.ModIngredients;
using QuiteEnoughRecipes.ModUIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.ItemDropRules;
using Terraria.UI;

namespace QuiteEnoughRecipes.ModRecipes;

public class WallDropsRecipe : IRecipe
{
	public required int WallType { get; set; }
	public required List<DropRateInfo> Drops { get; set; }

	public UIElement Element => new UIDropsPanel(new UIWallPanel(WallType, 70) { Border = 32 }, Drops);

	public IEnumerable<IIngredient> GetIngredients()
	{
		IEnumerable<IIngredient> item = [new WallIngredient(WallType)];
		var drops = Drops.Select(d => new ItemIngredient(new(d.itemId)) as IIngredient);
		return item.Concat(drops);
	}
}
