using QuiteEnoughRecipes.ModRecipeHandlers;
using Terraria;
using Terraria.UI;

namespace QuiteEnoughRecipes.ModUIElements;

public class UIWandsRecipePanel : UIAutoExtend
{
	public UIWandsRecipePanel(int wandItem, FlexibleTileWand_OptionBucket wandOptions)
	{
		Width.Percent = 1f;
		float xOffset = 0;
		var appendElement = (UIElement elem, float width) =>
		{
			elem.Left.Pixels = xOffset;
			Append(elem);
			xOffset += width + 10;
		};

		var wandPanel = new UIRecipeResultPanel(new Item(wandItem), 70)
		{
			VAlign = 0.5f
		};
		appendElement(wandPanel, 70);

		var consumedItemPanel = new UIRecipeResultPanel(new Item(wandOptions.ItemTypeToConsume), 30)
		{
			VAlign = 0.5f
		};
		appendElement(consumedItemPanel, 30);

		var grid = new UIAutoExtendGrid()
		{
			Width = new StyleDimension(-xOffset, 1),
			VAlign = 0.5f,
			HAlign = 1
		};

		foreach (var drop in wandOptions.Options)
		{
			var element = new UITilePanel(drop.TileIdToPlace, drop.TileStyleToPlace);
			grid.Append(element);
		}
		Append(grid);
	}
}
