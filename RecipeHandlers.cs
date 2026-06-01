using System.Collections.Generic;
using System.Linq;
using System;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria;
using Terraria.GameContent;
using System.Runtime.CompilerServices;

namespace QuiteEnoughRecipes;

public static class RecipeHandlers
{
	// Normal recipes.
	public class Basic : IRecipeHandler
	{
		public LocalizedText HoverName { get; }
			= Language.GetText("Mods.QuiteEnoughRecipes.Tabs.Recipes");

		public Item TabItem { get; } = new(ItemID.WorkBench);

		public IEnumerable<IRecipe> GetRecipes(IIngredient ing, QueryType queryType)
		{
			if (!(ing is ItemIngredient i)) { yield break; }

			foreach (var r in Main.recipe)
			{
				if (queryType == QueryType.Sources && r.createItem.type == i.Item.type
					|| queryType == QueryType.Uses && RecipeAcceptsItem(r, i.Item))
				{
					yield return new BasicRecipe(r);
				}
			}
		}

		public IEnumerable<Type> GetIngredientTypes() => [typeof(ItemIngredient)];
	}

	// Recipes requiring a crafting station.
	public class CraftingStations : IRecipeHandler
	{
		public LocalizedText HoverName { get; }
			= Language.GetText("Mods.QuiteEnoughRecipes.Tabs.Tiles");

		public Item TabItem { get; } = new(ItemID.Furnace);

		public IEnumerable<IRecipe> GetRecipes(IIngredient ing, QueryType queryType)
		{
			if (!(queryType == QueryType.Uses && ing is ItemIngredient i))
			{
				yield break;
			}

			foreach (var r in Main.recipe)
			{
				if (r.requiredTile.Contains(i.Item.createTile))
				{
					yield return new BasicRecipe(r);
				}
			}
		}

		public IEnumerable<Type> GetIngredientTypes() => [typeof(ItemIngredient)];
	}

	public class ShimmerTransmutations : IRecipeHandler
	{
		public LocalizedText HoverName { get; }
			= Language.GetText("Mods.QuiteEnoughRecipes.Tabs.Shimmer");
		
		public Item TabItem { get; } = new(ItemID.BottomlessShimmerBucket);

		public IEnumerable<IRecipe> GetRecipes(IIngredient ing, QueryType queryType)
		{
			if (!(ing is ItemIngredient i)) { yield break; }

			if (queryType == QueryType.Sources)
			{
				// coin luck takes precedence over other shimmer transforms.
				// short circuited to prevent coins from show up as part of decrafting
				var coinLuck = ItemID.Sets.CoinLuckValue[i.Item.type];
				if (coinLuck > 0)
					yield break;

				for (int id = 0; id < ItemID.Sets.ShimmerTransformToItem.Length; ++id)
				{
					if (ShimmerTransformResult(id) == i.Item.type)
					{
						yield return new BasicRecipe{
							Result = new(i.Item.type),
							RequiredItems = [new(id)]
						};
					}
				}

				for (int id = 0; id < ItemID.Sets.CraftingRecipeIndices.Length; ++id)
				{
					var item = new Item(id);

					var decraftRecipes = DecraftVanillaRecipes(item)
						.Where(r => ResultsContainItem(r.customShimmerResults ?? r.requiredItem, i.Item))
						.ToDecraftBasicRecipes()
						.DeduplicateDecraftRecipes();

					foreach (var recipe in decraftRecipes)
					{
						InsertDecraftCondition(recipe);
						yield return recipe;
					}
				}
			}
			else
			{
				var coinLuck = ItemID.Sets.CoinLuckValue[i.Item.type];
				if (coinLuck > 0)
				{
					yield return new BasicRecipe
					{
						Result = new(i.Item.type),
						Conditions = [CoinLuckCondition(coinLuck)]
					};
				}
				else if (ShimmerTransformResult(i.Item.type) is int id and not -1)
				{
					yield return new BasicRecipe
					{
						Result = new(id),
						RequiredItems = [new(i.Item.type)]
					};
				}
				else
				{
					var decraftRecipes = DecraftVanillaRecipes(i.Item)
						.ToDecraftBasicRecipes()
						.DeduplicateDecraftRecipes();
					foreach (var recipe in decraftRecipes)
					{
						InsertDecraftCondition(recipe);
						yield return recipe;
					}
				}
			}
		}

		public IEnumerable<Type> GetIngredientTypes() => [typeof(ItemIngredient)];
	}

	public class NPCShops : IRecipeHandler
	{
		public LocalizedText HoverName { get; }
			= Language.GetText("Mods.QuiteEnoughRecipes.Tabs.Shops");

		public Item TabItem { get; } = new(ItemID.GoldCoin);

		public IEnumerable<IRecipe> GetRecipes(IIngredient ing, QueryType queryType)
		{
			switch (ing, queryType)
			{
			case (ItemIngredient i, QueryType.Sources):
				foreach (var shop in NPCShopDatabase.AllShops)
				{
					if (shop.ActiveEntries.Any(e => e.Item.type == i.Item.type))
					{
						yield return new NPCShopRecipe{ Shop = shop };
					}
				}

				break;

			case (ItemIngredient i, QueryType.Uses):
				foreach (var shop in NPCShopDatabase.AllShops)
				{
					if (shop.ActiveEntries.Any(e => MatchesCurrency(i.Item, e.Item)))
					{
						yield return new NPCShopRecipe{ Shop = shop };
					}
				}

				break;

			case (NPCIngredient n, QueryType.Uses):
				foreach (var shop in NPCShopDatabase.AllShops)
				{
					if (shop.NpcType == n.ID)
					{
						yield return new NPCShopRecipe{ Shop = shop };
					}
				}

				break;
			}
		}

		public IEnumerable<Type> GetIngredientTypes() =>
			[typeof(ItemIngredient), typeof(NPCIngredient)];

		public static bool MatchesCurrency(Item currency, Item shopEntry)
		{
			if (CustomCurrencyManager.TryGetCurrencySystem(shopEntry.shopSpecialCurrency, out var customCurrency))
			{
				return customCurrency.Accepts(currency);
			}
			return currency.IsACoin;
		}
	}

	// Result of opening loot items like treasure bags and goodie bags.
	public class ItemDrops : IRecipeHandler
	{
		public LocalizedText HoverName { get; }
			= Language.GetText("Mods.QuiteEnoughRecipes.Tabs.ItemDrops");

		public Item TabItem { get; } = new(ItemID.CultistBossBag);

		public IEnumerable<IRecipe> GetRecipes(IIngredient ing, QueryType queryType)
		{
			if (!(ing is ItemIngredient i)) { yield break; }

			if (queryType == QueryType.Sources)
			{
				var item = new Item();
				for (int itemID = 0; itemID < ItemLoader.ItemCount; ++itemID)
				{
					item.SetDefaults(itemID);

					var droppedItems = GetItemDrops(item.type);
					if (droppedItems.Any(info => info.itemId == i.Item.type))
					{
						yield return new ItemDropsRecipe{
							Item = item.Clone(),
							Drops = droppedItems
						};
					}
				}
			}
			else
			{
				var droppedItems = GetItemDrops(i.Item.type);
				if (droppedItems.Count > 0)
				{
					yield return new ItemDropsRecipe{
						Item = i.Item,
						Drops = droppedItems
					};
				}
			}
		}

		public IEnumerable<Type> GetIngredientTypes() => [typeof(ItemIngredient)];
	}

	// Items dropped by NPCs when they are killed.
	public class NPCDrops : IRecipeHandler
	{
		public LocalizedText HoverName { get; }
			= Language.GetText("Mods.QuiteEnoughRecipes.Tabs.NPCDrops");

		public Item TabItem { get; } = new(ItemID.ZombieArm);

		public IEnumerable<IRecipe> GetRecipes(IIngredient ing, QueryType queryType)
		{
			switch (ing, queryType)
			{
			case (ItemIngredient i, QueryType.Sources):
				for (int id = 0; id < NPCLoader.NPCCount; ++id)
				{
					/*
					 * TODO: Is there a better way to check whether a bestiary entry exists?
					 *
					 * For some reason, `FindEntryByNPCID` doesn't return null, but *does*
					 * return an entry with a null icon.
					 */
					if (Main.BestiaryDB.FindEntryByNPCID(id).Icon == null) { continue; }

					var droppedItems = GetNPCDrops(id);
					if (droppedItems.Any(info => info.itemId == i.Item.type))
					{
						yield return new NPCDropsRecipe{
							NPCID = id,
							Drops = droppedItems
						};
					}
				}

				break;

			case (NPCIngredient n, QueryType.Uses):
				if (Main.BestiaryDB.FindEntryByNPCID(n.ID).Icon == null) { yield break; }

				var drops = GetNPCDrops(n.ID);
				if (drops.Count > 0)
				{
					yield return new NPCDropsRecipe{
						NPCID = n.ID,
						Drops = drops
					};
				}

				break;
			}
		}

		public IEnumerable<Type> GetIngredientTypes() =>
			[typeof(ItemIngredient), typeof(NPCIngredient)];
	}

	// If the item can be dropped from *any* NPC, this tab will show.
	public class GlobalDrops : IRecipeHandler
	{
		public LocalizedText HoverName { get; }
			= Language.GetText("Mods.QuiteEnoughRecipes.Tabs.GlobalDrops");

		public Item TabItem { get; } = new(ItemID.Heart);

		public IEnumerable<IRecipe> GetRecipes(IIngredient ing, QueryType queryType)
		{
			if (!(queryType == QueryType.Sources && ing is ItemIngredient i))
			{
				yield break;
			}

			var drops = GetGlobalDrops();
			if (drops.Any(info => info.itemId == i.Item.type))
			{
				yield return new GlobalDropsRecipe{ Drops = drops };
			}
		}

		public IEnumerable<Type> GetIngredientTypes() => [typeof(ItemIngredient)];
	}

	private static bool RecipeAcceptsItem(Recipe r, Item i)
	{
		return r.requiredItem.Any(x => x.type == i.type)
			|| r.acceptedGroups.Any(
				g => RecipeGroup.recipeGroups.TryGetValue(g, out var rg) && rg.ContainsItem(i.type)
			);
	}

	/*
	 * Returns the item ID of the shimmer result of transforming an item with ID `inputItem`. If no
	 * such output exists, returns -1.
	 */
	private static int ShimmerTransformResult(int inputItem)
	{
		int id = ItemID.Sets.ShimmerCountsAsItem[inputItem];
		if (id == -1) { id = inputItem; }
		return ItemID.Sets.ShimmerTransformToItem[id];
	}

	[UnsafeAccessor(UnsafeAccessorKind.Method, Name = "GetShimmerEquivalentType")]
	internal static extern int Item_GetShimmerEquivalentType(Item self);

	[UnsafeAccessor(UnsafeAccessorKind.Method, Name = "FindDecraftAmount")]
	internal static extern int Item_FindDecraftAmount(Item self);

	/*
	 * Given an input item, returns all the recipes that are used to craft that item with respect to decrafting
	 * Disabled recipes are omitted.
	 */
	private static IEnumerable<Recipe> DecraftVanillaRecipes(Item inputItem)
	{
		int shimmerEquivalentType = Item_GetShimmerEquivalentType(inputItem);
		foreach (int recipeIndex in ItemID.Sets.CraftingRecipeIndices[shimmerEquivalentType])
		{
			var recipe = Main.recipe[recipeIndex];
			if (!recipe.DecraftDisabled)
				yield return recipe;
		}
	}

	/*
	 * Applies the TML decraft ingredient consumption logic to a target recipe.
	 * The list of items contains the modified stack sizes.
	 */
	private static List<Item> ConsumedShimmerItems(Recipe shimmerRecipe)
	{
		// this is Item.FindDecraftAmount() in vanilla, but stack size should always be 1 for QER?
		int decraftAmount = 1;
		var requiredItems = shimmerRecipe.customShimmerResults ?? shimmerRecipe.requiredItem;

		List<Item> decraftItems = [];
		foreach (var item in requiredItems)
		{
			if (item.type <= 0) break;

			int stack = decraftAmount * item.stack;
			RecipeLoader.ConsumeIngredient(shimmerRecipe, item.type, ref stack, isDecrafting: true);

			decraftItems.Add(new(item.type, stack));
		}
		return decraftItems;
	}

	private static bool ResultsContainItem(List<Item> results, Item item) => results.Any(i => i.type == item.type);

	/*
	 * Converts a list of vanilla Recipes into QER BasicRecipes and modifies the result to include shimmer decrafting logic.
	 */
	private static IEnumerable<BasicRecipe> ToDecraftBasicRecipes(this IEnumerable<Recipe> vanillaRecipes)
	{
		foreach (var recipe in vanillaRecipes)
		{
			// some decrafts are locked behind progression, mostly for vanilla since decraft conditions exist
			// need to use createItem instead of inputItem since stack information could fail shimmer check
			// TODO: Perhaps exclude recipes that have decraft conditions for user awareness?
			if (!recipe.createItem.CanShimmer())
				continue;

			List<Item> decraftItems = ConsumedShimmerItems(recipe);

			yield return new BasicRecipe()
			{
				RequiredItems = decraftItems,
				Result = new(recipe.createItem.type, stack: recipe.createItem.stack),
				Conditions = recipe.DecraftConditions,
				SourceMod = recipe.Mod
			};
		}
	}

	/*
	 * Vanilla will pick the first recipe where the conditions are met. For awareness any recipe that has conditions should be listed.
	 * Vanilla recipes include mutually exclusive conditions like crimson/corruption, but mutual exclusion can not be relied upon.
	 * Thus listing all conditional recipes and the first unconditional recipe, should cover all possible results from shimmer decrafting.
	 */
	private static IEnumerable<BasicRecipe> DeduplicateDecraftRecipes(this IEnumerable<BasicRecipe> inputRecipes)
	{
		bool foundUnconditionalRecipe = false;
		foreach (var recipe in inputRecipes)
		{
			if (recipe.Conditions.Count > 0)
			{
				yield return recipe;
			}
			else if (!foundUnconditionalRecipe)
			{
				yield return recipe;
				foundUnconditionalRecipe = true;
			}
		}
	}

	public static readonly Condition Decrafting = new Condition("Mods.QuiteEnoughRecipes.Conditions.Decrafting", () => true);
	private static void InsertDecraftCondition(BasicRecipe recipe)
	{
		recipe.Conditions = new(recipe.Conditions);
		recipe.Conditions.Insert(0, Decrafting);
	}

	// Get items that can be dropped when using the item with ID `itemID`.
	private static List<DropRateInfo> GetItemDrops(int itemID)
	{
		return GetDropsFromRules(Main.ItemDropsDB.GetRulesForItemID(itemID));
	}

	// Get items that will be listed as drops from an NPC with ID `id`.
	private static List<DropRateInfo> GetNPCDrops(int id)
	{
		var bestiaryDrops = GetDropsFromRules(Main.ItemDropsDB.GetRulesForNPCID(id, false));
		var bannerDrop = GetBannerDrop(id);

		/*
		 * TODO: Prepending the banner is slow, so do this a different way.
		 *
		 * The banner needs to be prepended to make sure it's the first item in the `UIDropsPanel`.
		 * What should probably *eventually* happen is that `UIDropsPanel` should, on its own, sort
		 * banners before other items. However, it's actually kind of hard to tell if an item is a
		 * banner because there doesn't seem to be an `ItemToBanner` function.
		 */
		if (bannerDrop != null) { bestiaryDrops.Insert(0, bannerDrop.Value); }
		return bestiaryDrops;
	}

	private static List<DropRateInfo> GetGlobalDrops()
	{
		return GetDropsFromRules(new GlobalLoot(Main.ItemDropsDB).Get());
	}

	private static List<DropRateInfo> GetDropsFromRules(IEnumerable<IItemDropRule> rules)
	{
		/*
		 * TODO: It would be much more efficient to fill an existing list rather than making a new
		 * one each time.
		 */
		var results = new List<DropRateInfo>();
		var feed = new DropRateInfoChainFeed(1);

		foreach (var rule in rules)
		{
			rule.ReportDroprates(results, feed);
		}

		results.RemoveAll(info => info.conditions?.Any(c => !c.CanShowItemDropInUI()) ?? false);
		return results;
	}

	private record BannerDropCondition(int Kills) : IItemDropRuleCondition
	{
		public bool CanDrop(DropAttemptInfo info) => true;

		public bool CanShowItemDropInUI() => true;

		public string GetConditionDescription() => Language.GetText("Mods.QuiteEnoughRecipes.Conditions.BannerDrop").WithFormatArgs(Kills).Value;
	}
	private static DropRateInfo? GetBannerDrop(int id)
	{
		var banner = Item.NPCtoBanner(id);
		if (banner == 0) return null;
		var bannerItem = Item.BannerToItem(banner);
		var killRequirement = ItemID.Sets.KillsToBanner[bannerItem];
		return new DropRateInfo(bannerItem, 1, 1, 1, [new BannerDropCondition(killRequirement)]);
	}

	private static Condition CoinLuckCondition(int amount) =>
		new(Language.GetText("Mods.QuiteEnoughRecipes.Conditions.CoinLuck").WithFormatArgs(amount.ToString("N0")), () => true);
}
