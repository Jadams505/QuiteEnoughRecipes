using QuiteEnoughRecipes.ModRecipes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.UI;

namespace QuiteEnoughRecipes.ModUIElements;

public class UICaptureToolRecipePanel : UIElement
{
	public static RecipeGroup? AnyBugNet;

	public UICaptureToolRecipePanel(int catchItem, int npcId)
	{
		AnyBugNet ??= new
		(
			() => "Any Bug Net",
			CatchingTools().ToArray()
		);

		var result = new Item(catchItem);

		Height.Pixels = 72;
		Width.Percent = 1f;

		float offset = 0;

		var appendElement = (UIElement elem, float width) => {
			elem.Left.Pixels = offset;
			Append(elem);
			offset += width + 10;
		};

		var createItem = new UIRecipeResultPanel(result, 52)
		{
			VAlign = 0.5f
		};
		appendElement(createItem, 52);

		var bugNets = new UIRecipeGroupPanel(AnyBugNet, width: 30)
		{
			VAlign = 0.5f
		};
		appendElement(bugNets, 30);

		var entry = BestiaryDatabaseNPCsPopulator.FindEntryByNPCID(npcId);
		var npcElement = new UINPCPanel(npcId, 72)
		{
			VAlign = 0.5f
		};
		appendElement(npcElement, 72);
	}

	public static IEnumerable<int> CatchingTools() => ItemID.Sets.CatchingTool
		.Select((value, index) => (value, index))
		.Where(entry => entry.value)
		.Select(entry => entry.index);
}
