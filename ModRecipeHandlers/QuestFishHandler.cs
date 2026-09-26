using QuiteEnoughRecipes.ModRecipes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ID;
using Terraria.Localization;

namespace QuiteEnoughRecipes.ModRecipeHandlers;

public class QuestFishHandler : IRecipeHandler
{
	public LocalizedText HoverName { get; }
		= Language.GetText("Mods.QuiteEnoughRecipes.Tabs.QuestFish");
	public Item TabItem { get; } = new Item(ItemID.Bunnyfish);

	public IEnumerable<Type> GetIngredientTypes() => [typeof(ItemIngredient), typeof(NPCIngredient)];

	public IEnumerable<IRecipe> GetRecipes(IIngredient ing, QueryType queryType)
	{
		if (ing is NPCIngredient npcIng)
		{
			if (queryType is not QueryType.Uses) yield break;
			if (npcIng.ID is not NPCID.Angler) yield break;

			var questFish = ContentSamples.ItemsByType.Where(e => e.Value.questItem).Select(e => e.Key);
			yield return new ResultDropsRecipe<UINPCPanel, UIItemPanel>()
			{
				Drops = questFish.Select(i => new UIItemPanel(new(i))),
				Result = new UINPCPanel(npcIng.ID)
			};
		}
		else if (ing is ItemIngredient itemIng)
		{
			// should this be source or use?
			if (queryType is not QueryType.Sources) yield break;

			var questFish = ContentSamples.ItemsByType.Where(e => e.Value.questItem).Select(e => e.Key);
			if (questFish.Contains(itemIng.Item.type))
			{
				yield return new ResultDropsRecipe<UINPCPanel, UIItemPanel>()
				{
					Drops = questFish.Select(i => new UIItemPanel(new(i))),
					Result = new UINPCPanel(NPCID.Angler)
				};
			}
		}
	}
}
