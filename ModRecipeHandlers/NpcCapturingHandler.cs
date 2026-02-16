using QuiteEnoughRecipes.ModRecipes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;

namespace QuiteEnoughRecipes.ModRecipeHandlers;

public class NpcCapturingHandler : IRecipeHandler
{
	public LocalizedText HoverName { get; }
		= Language.GetText("Mods.QuiteEnoughRecipes.Tabs.Catching");
	public Item TabItem { get; } = new(ItemID.BugNet);

	public IEnumerable<Type> GetIngredientTypes() =>
		[typeof(ItemIngredient)];

	public IEnumerable<IRecipe> GetRecipes(IIngredient ing, QueryType queryType) => (ing, queryType) switch
	{
		(ItemIngredient itemIng, QueryType.Uses) when ItemID.Sets.CatchingTool[itemIng.Item.type] => 
			GetAllCatchableNpcs()
				.Select(npc => new CaptureItemRecipe(npc)),
		(ItemIngredient itemIng, QueryType.Sources) when GetAllCatchableItems().Contains(itemIng.Item.type) =>
			GetAllCatchableNpcs()
				.Where(npc => npc.catchItem == itemIng.Item.type)
				.Select(npc => new CaptureItemRecipe(npc)),
		(NPCIngredient npcIng, QueryType.Uses) when GetAllCatchableNpcs().Any(npc => npc.type == npcIng.ID) =>
			GetAllCatchableNpcs()
				.Where(npc => npc.type == npcIng.ID)
				.Select(npc => new CaptureItemRecipe(npc)),
		_ => []
	};

	public static IEnumerable<NPC> GetAllCatchableNpcs() =>
		ContentSamples.NpcsByNetId.Values.Where(n => n.catchItem > 0);

	public static IEnumerable<int> GetAllCatchableItems() =>
		ContentSamples.NpcsByNetId.Values.Where(n => n.catchItem > 0).Select(n => n.catchItem);
}
