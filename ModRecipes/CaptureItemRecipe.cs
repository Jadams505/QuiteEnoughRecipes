using QuiteEnoughRecipes.ModUIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;

namespace QuiteEnoughRecipes.ModRecipes;

public class CaptureItemRecipe : IRecipe
{
	public int Result { get; init; }
	public int NPCID { get; init; }

	public CaptureItemRecipe(NPC npc)
	{
		NPCID = npc.type;
		Result = npc.catchItem;
	}

	public UIElement Element => new UICaptureToolRecipePanel(Result, NPCID);

	public IEnumerable<IIngredient> GetIngredients()
	{
		yield return new ItemIngredient(new(Result));
		
		foreach (var item in CatchingTools())
		{
			yield return new ItemIngredient(new(item));
		}

		yield return new NPCIngredient(NPCID);
	}

	public static IEnumerable<int> CatchingTools() => ItemID.Sets.CatchingTool
		.Select((value, index) => (value, index))
		.Where(entry => entry.value)
		.Select(entry => entry.index);
}
