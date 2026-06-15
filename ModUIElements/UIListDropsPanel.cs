using System.Collections.Generic;
using Terraria.UI;

namespace QuiteEnoughRecipes.ModUIElements;
public class UIListDropsPanel<T> : UIAutoExtend where T : UIElement
{
	public UIListDropsPanel(UIElement left, IEnumerable<T> dropElements)
	{
		Width.Percent = 1;

		var grid = new UIAutoExtendGrid()
		{
			Width = new StyleDimension(-left.Width.Pixels - 10, 1 - left.Width.Percent),
			HAlign = 1
		};

		foreach (var drop in dropElements)
		{
			grid.Append(drop);
		}

		Append(left);
		Append(grid);
	}
}
