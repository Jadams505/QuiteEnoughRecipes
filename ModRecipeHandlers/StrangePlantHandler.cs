using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace QuiteEnoughRecipes.ModRecipeHandlers;

public class StrangePlantHandler : IRecipeHandler
{
	public LocalizedText HoverName { get; }
		= Language.GetText("Mods.QuiteEnoughRecipes.Tabs.StrangePlant");

	public Item TabItem { get; } = new Item(ItemID.StrangePlant1);

	public IEnumerable<Type> GetIngredientTypes() => [typeof(ItemIngredient)];

	public IEnumerable<IRecipe> GetRecipes(IIngredient ing, QueryType queryType)
	{
		var chests = Main.chest;

		return (ing, queryType) switch
		{
			(ItemIngredient item, QueryType.Uses) when ItemID.Sets.ExoticPlantsForDyeTrade[item.Item.type] => GetUsageRecipes(),
			(NPCIngredient npc, QueryType.Uses) when npc.ID is NPCID.DyeTrader => GetUsageRecipes(),
			(ItemIngredient item, QueryType.Sources) => GetSourceRecipes(item.Item),
			_ => []
		};

		static IEnumerable<IRecipe> GetUsageRecipes()
		{
			var items = Player_GetDyeTraderReward_Patch.GetDyeTraderReward(Main.LocalPlayer);
			float rewardChance = items.Count > 0 ? 1f / items.Count : 0;
			yield return new NPCDropsRecipe()
			{
				NPCID = NPCID.DyeTrader,
				Drops = items.Select(i => new DropRateInfo(i, 6, 6, rewardChance)).ToList()
			};
		}

		static IEnumerable<IRecipe> GetSourceRecipes(Item source)
		{
			var rewards = Player_GetDyeTraderReward_Patch.GetDyeTraderReward(Main.LocalPlayer);
			foreach (var item in rewards)
			{
				if (item == source.type)
				{
					foreach (var plant in GetStrangePlants())
					{
						yield return new BasicRecipe()
						{
							Result = new Item(item, 6),
							RequiredItems = [new(plant)],
							Conditions = [DyeTraderCondition]
						};
					}
				}
			}
		}
	}

	public static readonly Condition DyeTraderCondition = new("Mods.QuiteEnoughRecipes.Conditions.DyeTrader", () => true);

	public static IEnumerable<int> GetStrangePlants()
	{
		for (int i = 0; i < ItemID.Sets.ExoticPlantsForDyeTrade.Length; ++i)
		{
			if (ItemID.Sets.ExoticPlantsForDyeTrade[i])
				yield return i;
		}
	}
}

public class Player_GetDyeTraderReward_Patch : ModSystem
{
	internal static bool ReadonlyMode { get; set; } = false;
	internal static List<int> RewardPoolCache = [];

	public override void Load()
	{
		IL_Player.GetDyeTraderReward += IL_Player_GetDyeTraderReward;
	}

	public static List<int> GetDyeTraderReward(Player player)
	{
		var dummyDyeTrader = new NPC();
		dummyDyeTrader.SetDefaults(NPCID.DyeTrader);
		ReadonlyMode = true;
		player.GetDyeTraderReward(dummyDyeTrader);
		ReadonlyMode = false;
		return RewardPoolCache;
	}

	private void IL_Player_GetDyeTraderReward(ILContext il)
	{
		var c = new ILCursor(il);

		int thisIndex = -1;
		int rewardPoolIndex = -1;
		bool foundGetDyeTraderReward = c.TryGotoNext(MoveType.After,
			x => x.MatchLdarg(out thisIndex),
			x => x.MatchLdloc(out rewardPoolIndex),
			i => i.MatchCall(typeof(PlayerLoader), nameof(PlayerLoader.GetDyeTraderReward))
		);

		if (!foundGetDyeTraderReward)
		{
			Mod.Logger.Warn($"{nameof(foundGetDyeTraderReward)} failed. Aborting IL edits");
			return;
		}

		try
		{
			var returnLabel = c.DefineLabel();

			c.EmitLdloc(rewardPoolIndex);
			c.EmitDelegate(ReadonlyGetDyeTraderReward);
			c.Emit(OpCodes.Brfalse, returnLabel);
			c.EmitRet();

			c.MarkLabel(returnLabel);
		}
		catch (Exception e)
		{
			Mod.Logger.Warn($"{nameof(foundGetDyeTraderReward)} failed. Aborting IL edits");
		}
	}

	private static bool ReadonlyGetDyeTraderReward(List<int> rewardPool)
	{
		RewardPoolCache = rewardPool;
		return ReadonlyMode;
	}
}
