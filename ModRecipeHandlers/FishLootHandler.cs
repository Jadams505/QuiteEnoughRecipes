using QuiteEnoughRecipes.ModRecipeGroups;
using QuiteEnoughRecipes.ModRecipes;
using QuiteEnoughRecipes.ModUIElements;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.GameContent.FishDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace QuiteEnoughRecipes.ModRecipeHandlers;

public class FishLootHandler : IRecipeHandler
{
	internal Dictionary<AFishingCondition, List<FishDropRule>> CachedRuleLookup 
	{ 
		get => field ??= CreateLookup(); 
	}

	public LocalizedText HoverName { get; }
		= Language.GetText("Mods.QuiteEnoughRecipes.Tabs.FishLoot");
	public Item TabItem { get; } = new(ItemID.ReinforcedFishingPole);

	public IEnumerable<Type> GetIngredientTypes() => [typeof(ItemIngredient)];

	public IEnumerable<IRecipe> GetRecipes(IIngredient ing, QueryType queryType)
	{
		if (ing is not ItemIngredient itemIng) yield break;

		if (queryType is QueryType.Sources)
		{
			foreach (var pair in CachedRuleLookup)
			{
				var flattenedItems = pair.Value.SelectMany(r => r.PossibleItems);
				if (flattenedItems.Contains(itemIng.Item.type))
				{
					var drops = pair.Value.SelectMany(r => r.PossibleItems, (r, i) => new UIFishLootItemPanel(r, i)).DistinctBy(e => e.DisplayedItem?.type);
					yield return new ResultDropsRecipe<UIFishConditionPanel, UIFishLootItemPanel>()
					{
						Drops = drops,
						Result = new(pair.Key)
					};
				}
			}
		}
		else if (queryType is QueryType.Uses)
		{
			// So this is technically wrong not all fishing rods can fish under all conditions, most notably lava fishing
			// But with lava bait they can, so not sure how to handle
			if (RecipeGroupSystem.AllFishingRods.ValidItems().Contains(itemIng.Item.type))
			{
				foreach (var pair in CachedRuleLookup)
				{
					var drops = pair.Value.SelectMany(r => r.PossibleItems, (r, i) => new UIFishLootItemPanel(r, i)).DistinctBy(e => e.DisplayedItem?.type);
					yield return new ResultDropsRecipe<UIFishConditionPanel, UIFishLootItemPanel>()
					{
						Drops = drops,
						Result = new(pair.Key)
					};
				}
			}
		}
	}

	private static Dictionary<AFishingCondition, List<FishDropRule>> CreateLookup()
	{
		var db = FishDropsSystem.QERFishDropsDB;
		Dictionary<AFishingCondition, List<FishDropRule>> rulesByCondition = db.Rules
			.SelectMany(rule => rule.Conditions.Select(condition => new
			{
				Condition = condition,
				Rule = rule
			}))
			.GroupBy(x => x.Condition, AFishingConditionComparer.Instance)
			.ToDictionary(
				group => group.Key,
				group => group.Select(x => x.Rule).ToList(),
				AFishingConditionComparer.Instance
		);

		// Add entries that have no rules
		var emptyCondition = new NoRulesFishingCondition();
		rulesByCondition.Add(new QERFishDropRulesPopulator.QERFishingCondition()
		{
			Condition = emptyCondition,
			Description = LocalizedText.Empty
		}, db.Rules.Where(rule => rule.Conditions.Length == 0).ToList());
		return rulesByCondition;
	}
}

public static class FishDropRuleList_Extensions
{
	[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_rules")]
	public static extern ref List<FishDropRule> FishDropRuleList_rules(FishDropRuleList self);

	extension(FishDropRuleList self)
	{
		public List<FishDropRule> Rules => FishDropRuleList_rules(self);
	}
}

public class AFishingConditionComparer : IEqualityComparer<AFishingCondition>
{
	public static AFishingConditionComparer Instance = new();

	public bool Equals(AFishingCondition? x, AFishingCondition? y)
	{
		if (x is QERFishDropRulesPopulator.QERFishingCondition QERx && y is QERFishDropRulesPopulator.QERFishingCondition QERy)
		{
			return QERx.Description.Value == QERy.Description.Value;
		}
		return x.Equals(y);
	}

	public int GetHashCode([DisallowNull] AFishingCondition obj)
	{
		return obj.GetHashCode();
	}
}

public class QERFishDropRulesPopulator : GameContentFishDropPopulator
{
	public class QERFishingCondition : AFishingCondition
	{
		public required AFishingCondition Condition 
		{ 
			get;
			init
			{
				field = value;
				CanBeSkippedForDisplay = value.CanBeSkippedForDisplay;
			} 
		}

		public required LocalizedText Description { get; init; }

		public override bool Matches(FishingContext context) => Condition.Matches(context);

		public override bool Equals(object? obj)
		{
			if (obj is QERFishingCondition qer)
			{
				return qer.Description.Value == Description.Value;
			}
			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return Description.GetHashCode();
		}
	}

	public class QERFishingRarity : FishRarityCondition
	{
		public required FishRarityCondition Condition 
		{ 
			get; 
			init
			{
				field = value;
				FrequencyOfAppearanceForVisuals = value.FrequencyOfAppearanceForVisuals;
				HackedIsAny = value.HackedIsAny;
			} 
		}

		public required LocalizedText Description { get; init; }

		public override bool Matches(FishingContext context)
		{
			return Condition.Matches(context);
		}

		public override bool Equals(object? obj)
		{
			if (obj is QERFishingRarity qer)
			{
				return qer.Description.Value == Description.Value;
			}
			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return Description.GetHashCode();
		}
	}

	public class QERRarity
	{
		public static FishRarityCondition Any = new QERFishingRarity()
		{
			Condition = Rarity.Any,
			Description = Language.GetOrRegister("Mods.QuiteEnoughRecipes.Conditions.AnyRarity")
		};
		public static FishRarityCondition Legendary = new QERFishingRarity()
		{
			Condition = Rarity.Legendary,
			Description = Language.GetOrRegister("Mods.QuiteEnoughRecipes.Conditions.LegendaryRarity")
		};
		public static FishRarityCondition VeryRare = new QERFishingRarity()
		{
			Condition = Rarity.VeryRare,
			Description = Language.GetOrRegister("Mods.QuiteEnoughRecipes.Conditions.VeryRareRarity")
		};
		public static FishRarityCondition Rare = new QERFishingRarity()
		{
			Condition = Rarity.Rare,
			Description = Language.GetOrRegister("Mods.QuiteEnoughRecipes.Conditions.RareRarity")
		};
		public static FishRarityCondition Uncommon = new QERFishingRarity()
		{
			Condition = Rarity.Uncommon,
			Description = Language.GetOrRegister("Mods.QuiteEnoughRecipes.Conditions.UncommonRarity")
		};
		public static FishRarityCondition Common = new QERFishingRarity()
		{
			Condition = Rarity.Common,
			Description = Language.GetOrRegister("Mods.QuiteEnoughRecipes.Conditions.CommonRarity")
		};

		public static FishRarityCondition BombRarityOfNotLegendaryAndNotVeryRareAndUncommon = new QERFishingRarity()
		{
			Condition = Rarity.BombRarityOfNotLegendaryAndNotVeryRareAndUncommon,
			Description = Language.GetOrRegister("Mods.QuiteEnoughRecipes.Conditions.BombRarityOfNotLegendaryAndNotVeryRareAndUncommon")
		};
		public static FishRarityCondition UncommonOrCommon = new QERFishingRarity()
		{
			Condition = Rarity.UncommonOrCommon,
			Description = Language.GetOrRegister("Mods.QuiteEnoughRecipes.Conditions.UncommonOrCommonRarity")
		};
	}

	protected FishDropRuleList RulesList { get; init; }

	public QERFishDropRulesPopulator(FishDropRuleList list) : base(list)
	{
		RulesList = list;
	}

	public void PrePopulate()
	{
		HardMode = new QERFishingCondition
		{
			Condition = HardMode,
			Description = Condition.Hardmode.Description
		};
		EarlyMode = new QERFishingCondition
		{
			Condition = EarlyMode,
			Description = Condition.PreHardmode.Description
		};
		InLava = new QERFishingCondition
		{
			Condition = InLava,
			Description = Language.GetOrRegister("Mods.QuiteEnoughRecipes.Conditions.InLava")
		};
		InHoney = new QERFishingCondition
		{
			Condition = InHoney,
			Description = Language.GetOrRegister("Mods.QuiteEnoughRecipes.Conditions.InHoney")
		};
		Junk = new QERFishingCondition
		{
			Condition = Junk,
			Description = Language.GetOrRegister("Mods.QuiteEnoughRecipes.Conditions.Junk")
		};
		Crate = new QERFishingCondition
		{
			Condition = Crate,
			Description = Language.GetOrRegister("Mods.QuiteEnoughRecipes.Conditions.Crate")
		};
		AnyEnemies = new QERFishingCondition
		{
			Condition = AnyEnemies,
			Description = Language.GetOrRegister("Mods.QuiteEnoughRecipes.Conditions.AnyEnemies")
		};
		CanFishInLava = new QERFishingCondition
		{
			Condition = CanFishInLava,
			Description = Language.GetOrRegister("Mods.QuiteEnoughRecipes.Conditions.CanFishInLava")
		};
		Dungeon = new QERFishingCondition
		{
			Condition = Dungeon,
			Description = Language.GetOrRegister("Mods.QuiteEnoughRecipes.Conditions.Dungeon")
		};
		Beach = new QERFishingCondition
		{
			Condition = Beach,
			Description = Language.GetOrRegister("Mods.QuiteEnoughRecipes.Conditions.Beach")
		};
		Hallow = new QERFishingCondition
		{
			Condition = Hallow,
			Description = Language.GetOrRegister("Mods.QuiteEnoughRecipes.Conditions.Hallow")
		};
		GlowingMushrooms = new QERFishingCondition
		{
			Condition = GlowingMushrooms,
			Description = Language.GetOrRegister("Mods.QuiteEnoughRecipes.Conditions.GlowingMushrooms")
		};
		TrueDesert = new QERFishingCondition
		{
			Condition = TrueDesert,
			Description = Language.GetOrRegister("Mods.QuiteEnoughRecipes.Conditions.TrueDesert")
		};
		TrueSnow = new QERFishingCondition
		{
			Condition = TrueSnow,
			Description = Language.GetOrRegister("Mods.QuiteEnoughRecipes.Conditions.TrueSnow")
		};
		Remix = new QERFishingCondition
		{
			Condition = Remix,
			Description = Language.GetOrRegister("Mods.QuiteEnoughRecipes.Conditions.Remix")
		};
		Height1 = new QERFishingCondition
		{
			Condition = Height1,
			Description = Language.GetOrRegister("Mods.QuiteEnoughRecipes.Conditions.Height1")
		};
		Height1And2 = new QERFishingCondition
		{
			Condition = Height1And2,
			Description = Language.GetOrRegister("Mods.QuiteEnoughRecipes.Conditions.Height1And2")
		};
		HeightAbove1 = new QERFishingCondition
		{
			Condition = HeightAbove1,
			Description = Language.GetOrRegister("Mods.QuiteEnoughRecipes.Conditions.HeightAbove1")
		};
		HeightAboveAnd1 = new QERFishingCondition
		{
			Condition = HeightAboveAnd1,
			Description = Language.GetOrRegister("Mods.QuiteEnoughRecipes.Conditions.HeightAboveAnd1")
		};
		HeightUnder2 = new QERFishingCondition
		{
			Condition = HeightUnder2,
			Description = Language.GetOrRegister("Mods.QuiteEnoughRecipes.Conditions.HeightUnder2")
		};
		HeightAbove2 = new QERFishingCondition
		{
			Condition = HeightAbove2,
			Description = Language.GetOrRegister("Mods.QuiteEnoughRecipes.Conditions.HeightAbove2")
		};
		Height0 = new QERFishingCondition
		{
			Condition = Height0,
			Description = Language.GetOrRegister("Mods.QuiteEnoughRecipes.Conditions.Height0")
		};
		Height2 = new QERFishingCondition
		{
			Condition = Height2,
			Description = Language.GetOrRegister("Mods.QuiteEnoughRecipes.Conditions.Height2")
		};
		Height3 = new QERFishingCondition
		{
			Condition = Height3,
			Description = Language.GetOrRegister("Mods.QuiteEnoughRecipes.Conditions.Height3")
		};
		UnderRockLayer = new QERFishingCondition
		{
			Condition = UnderRockLayer,
			Description = Language.GetOrRegister("Mods.QuiteEnoughRecipes.Conditions.UnderRockLayer")
		};
		Corruption = new QERFishingCondition
		{
			Condition = Corruption,
			Description = Language.GetOrRegister("Mods.QuiteEnoughRecipes.Conditions.Corruption")
		};
		Crimson = new QERFishingCondition
		{
			Condition = Crimson,
			Description = Language.GetOrRegister("Mods.QuiteEnoughRecipes.Conditions.Crimson")
		};
		Jungle = new QERFishingCondition
		{
			Condition = Jungle,
			Description = Language.GetOrRegister("Mods.QuiteEnoughRecipes.Conditions.Jungle")
		};
		Snow = new QERFishingCondition
		{
			Condition = Snow,
			Description = Language.GetOrRegister("Mods.QuiteEnoughRecipes.Conditions.Snow")
		};
		Desert = new QERFishingCondition
		{
			Condition = Desert,
			Description = Language.GetOrRegister("Mods.QuiteEnoughRecipes.Conditions.Desert")
		};
		RolledHallowDesert = new QERFishingCondition
		{
			Condition = RolledHallowDesert,
			Description = Language.GetOrRegister("Mods.QuiteEnoughRecipes.Conditions.RolledHallowDesert")
		};
		OriginalOcean = new QERFishingCondition
		{
			Condition = OriginalOcean,
			Description = Language.GetOrRegister("Mods.QuiteEnoughRecipes.Conditions.OriginalOcean")
		};
		RemixOcean = new QERFishingCondition
		{
			Condition = RemixOcean,
			Description = Language.GetOrRegister("Mods.QuiteEnoughRecipes.Conditions.RemixOcean")
		};
		Ocean = new QERFishingCondition
		{
			Condition = Ocean,
			Description = Language.GetOrRegister("Mods.QuiteEnoughRecipes.Conditions.Ocean")
		};
		Water1000 = new QERFishingCondition
		{
			Condition = Water1000,
			Description = Language.GetOrRegister("Mods.QuiteEnoughRecipes.Conditions.Water1000")
		};
		BloodMoon = new QERFishingCondition
		{
			Condition = BloodMoon,
			Description = Language.GetOrRegister("Mods.QuiteEnoughRecipes.Conditions.BloodMoon")
		};
		DidNotUseCombatBook = new QERFishingCondition
		{
			Condition = DidNotUseCombatBook,
			Description = Language.GetOrRegister("Mods.QuiteEnoughRecipes.Conditions.DidNotUseCombatBook")
		};
	}

	public void PostPopulate()
	{
		foreach (var rule in RulesList.Rules)
		{
			var conds = rule.Conditions;
			for (int i = 0; i < conds.Length; ++i)
			{
				var con = conds[i];
				if (con is FishingConditions.QuestFishCondition questCon)
				{
					conds[i] = new QERFishingCondition()
					{
						Condition = questCon,
						Description = Language.GetOrRegister("Mods.QuiteEnoughRecipes.Conditions.QuestFishCondition")
					};
				}

				else if (con is FishingConditions.QuestFishConditionRemix remixCon)
				{
					conds[i] = new QERFishingCondition()
					{
						Condition = remixCon,
						Description = Language.GetOrRegister("Mods.QuiteEnoughRecipes.Conditions.QuestRemixFishCondition")
					};
				}
			}

			SwapRarities(rule);
		}
	}

	public static void SwapRarities(FishDropRule rule)
	{
		if (rule.Rarity == Rarity.Any) rule.Rarity = QERRarity.Any;
		else if (rule.Rarity == Rarity.Legendary) rule.Rarity = QERRarity.Legendary;
		else if (rule.Rarity == Rarity.VeryRare) rule.Rarity = QERRarity.VeryRare;
		else if (rule.Rarity == Rarity.Rare) rule.Rarity = QERRarity.Rare;
		else if (rule.Rarity == Rarity.Uncommon) rule.Rarity = QERRarity.Uncommon;
		else if (rule.Rarity == Rarity.Common) rule.Rarity = QERRarity.Common;
		else if (rule.Rarity == Rarity.BombRarityOfNotLegendaryAndNotVeryRareAndUncommon)
			rule.Rarity = QERRarity.BombRarityOfNotLegendaryAndNotVeryRareAndUncommon;
		else if (rule.Rarity == Rarity.UncommonOrCommon) rule.Rarity = QERRarity.UncommonOrCommon;
	}
}

public class NoRulesFishingCondition : AFishingCondition
{
	public override bool Matches(FishingContext context)
	{
		return true;
	}
}

public class FishDropsSystem : ModSystem
{
	public static FishDropRuleList QERFishDropsDB = new();
	
	public override void PostSetupContent()
	{
		FishDropRuleList fishDropRuleList = new FishDropRuleList();
		var populator = new QERFishDropRulesPopulator(fishDropRuleList);
		populator.PrePopulate();
		populator.Populate();
		populator.PostPopulate();
		QERFishDropsDB = fishDropRuleList;
	}
}
