using QuiteEnoughRecipes.ModIngredients;
using QuiteEnoughRecipes.ModRecipes;
using QuiteEnoughRecipes.ModUIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.Localization;

namespace QuiteEnoughRecipes.ModRecipeHandlers;
public class BuffImmunitiesHandler : IRecipeHandler
{
	public LocalizedText HoverName { get; } =
		Language.GetText("Mods.QuiteEnoughRecipes.Tabs.Immunities");
	public Item TabItem { get; } = new(ItemID.AdhesiveBandage);

	public IEnumerable<Type> GetIngredientTypes() => [typeof(BuffIngredient), typeof(NPCIngredient)];

	public IEnumerable<IRecipe> GetRecipes(IIngredient ing, QueryType queryType) => (ing, queryType) switch
	{
		(NPCIngredient npcIng, QueryType.Uses) => ContentSamples.NpcsByNetId
			.Where(n => n.Key > 0 && npcIng.ID == n.Key && !NPCID.Sets.ImmuneToAllBuffs[n.Key] && !NPCID.Sets.ImmuneToRegularBuffs[n.Key] && n.Value.buffImmune.Any(immune => immune))
			.Select(entry => new ResultDropsRecipe<UINPCPanel, UIBuffPanel>
			{
				Result = new UINPCPanel(entry.Key, 72),
				Drops = ImmuneBuffs(entry.Value).Take(20).Select(buff => new UIBuffPanel(buff, 52)
				{
					Border = 16
				})
			}),
		(BuffIngredient buffIng, QueryType.Uses) => ContentSamples.NpcsByNetId
			.Select(entry => (npcId: entry.Key, immunities: ImmuneBuffs(entry.Value)))
			.Where(entry => entry.npcId > 0 && !NPCID.Sets.ImmuneToAllBuffs[entry.npcId] && !NPCID.Sets.ImmuneToRegularBuffs[entry.npcId] && entry.immunities.Contains(buffIng.BuffType))
			.Select(entry => new ResultDropsRecipe<UINPCPanel, UIBuffPanel> 
			{
				Result = new UINPCPanel(entry.npcId, 72),
				Drops = entry.immunities.Take(20).Select(buff => new UIBuffPanel(buff, 52)
				{
					Border = 16
				})
			}),
		_ => []
	};

	private static IEnumerable<int> ImmuneBuffs(NPC npc)
	{
		for (int i = 0; i < npc.buffImmune.Length; ++i)
		{
			if (npc.buffImmune[i])
				yield return i;
		}
	}
}
