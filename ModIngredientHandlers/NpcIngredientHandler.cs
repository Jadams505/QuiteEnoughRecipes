using QuiteEnoughRecipes.ModAPI;
using QuiteEnoughRecipes.ModRecipeHandlers;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace QuiteEnoughRecipes.ModIngredientHandlers;

public class NpcIngredientHandler : QERIngredientHandler
{
	public override LocalizedText HoverName { get; } = Language.GetText("Mods.QuiteEnoughRecipes.Tabs.NPCList");
	public override LocalizedText SearchHelp { get; } = Language.GetText("Mods.QuiteEnoughRecipes.UI.NPCSearchHelp");
	public override UIElement TabElement { get; } = ITab.ItemIcon(new Item(ItemID.Bunny));

	public override IQueryableSource<IIngredient> GetIngredients()
	{
		var allNPCs = Enumerable.Range(0, NPCLoader.NPCCount)
			.Where(n => Main.BestiaryDB.FindEntryByNPCID(n).Icon != null ||
				NpcCapturingHandler.GetAllCatchableNpcs().Any(npc => npc.type == n))
			.Select(n => new NPCIngredient(n))
			.ToList();

		return new UIQueryableIngredientGrid<NPCIngredient, UINPCPanel>(allNPCs.ToList());
	}
}