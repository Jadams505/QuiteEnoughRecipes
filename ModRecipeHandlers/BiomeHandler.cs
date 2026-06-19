using QuiteEnoughRecipes.ModIngredients;
using QuiteEnoughRecipes.ModRecipes;
using QuiteEnoughRecipes.ModUIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace QuiteEnoughRecipes.ModRecipeHandlers;
public class BiomeHandler : IRecipeHandler
{
	public LocalizedText HoverName { get; }
		= Language.GetText("Mods.QuiteEnoughRecipes.Tabs.Biome");

	public Item TabItem { get; } = new(ItemID.WorldGlobe);

	public IEnumerable<Type> GetIngredientTypes() => [typeof(BiomeIngredient), typeof(NPCIngredient)];

	public IEnumerable<IRecipe> GetRecipes(IIngredient ing, QueryType queryType) => (ing, queryType) switch
	{
		(NPCIngredient npcIng, QueryType.Uses) => [
			new ResultDropsRecipe<UINPCPanel, UIBiomePanel>()
			{
				Result = new UINPCPanel(npcIng.ID),
				Drops = GetBiomes(npcIng.ID)
						.DistinctBy(entry => entry.GetDisplayNameKey())
						.Select(entry => new UIBiomePanel(entry))
			}],
		(BiomeIngredient biomeIng, QueryType.Sources) => GetBiomeSourceRecipes(biomeIng),
		_ => []
	};

	public static IEnumerable<IRecipe> GetBiomeSourceRecipes(BiomeIngredient biomeIng)
	{
		for (int i = 0; i < NPCLoader.NPCCount; ++i)
		{
			var biomes = GetBiomes(i);

			if (!biomes.Any(b => b.GetDisplayNameKey() == biomeIng.InfoType.GetDisplayNameKey()))
				continue;

			yield return new ResultDropsRecipe<UINPCPanel, UIBiomePanel>()
			{
				Result = new UINPCPanel(i),
				Drops = biomes
						.DistinctBy(entry => entry.GetDisplayNameKey())
						.Select(entry => new UIBiomePanel(entry))
			};
		}
	}

	public static IEnumerable<IFilterInfoProvider> GetBiomes(int npc)
	{
		var entry = Main.BestiaryDB.FindEntryByNPCID(npc);
		return GetBiomes(entry.Info);
	}

	public static IEnumerable<IFilterInfoProvider> GetBiomes(IEnumerable<IBestiaryInfoElement> infos)
	{
		foreach (var info in infos)
		{
			if (info is ModBestiaryInfoElement modBiome and not ModSourceBestiaryInfoElement)
			{
				yield return modBiome;
			}
			else if (info is FilterProviderInfoElement filter)
			{
				yield return filter;
			}
		}
	}
}