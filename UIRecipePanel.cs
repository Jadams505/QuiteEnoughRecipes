using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Map;
using Terraria.UI;
using Terraria;

namespace QuiteEnoughRecipes;

// Displays a recipe; similar to what you might see in the crafting window.
public class UIRecipePanel : UIAutoExtend, IHighlightableElement
{
	protected Item CreateItem { get; }
	protected List<Item> RequiredItems { get; }
	protected List<int> AcceptedGroups { get; }
	protected List<int> RequiredTiles { get; }
	protected List<Condition> Conditions { get; }

	private readonly UIText _constraintText;

	/*
	 * Sometimes we want to show recipes that aren't real recipes (like shimmer), so we want to
	 * create a new `Recipe` object. However, Terraria will throw an exception if we try to
	 * construct a `Recipe` in the wrong context, so we instead have to use this stuff directly.
	 */
	public UIRecipePanel(Item createItem, List<Item>? requiredItems = null,
		List<int>? acceptedGroups = null, List<int>? requiredTiles = null,
		List<Condition>? conditions = null)
	{
		CreateItem = createItem;
		RequiredItems = requiredItems ?? [];
		AcceptedGroups = acceptedGroups ?? [];
		RequiredTiles = requiredTiles ?? [];
		Conditions = conditions ?? [];

		Height.Pixels = 50;
		Width.Percent = 1;

		float offset = 0;

		var appendElement = (UIElement elem, float width) => {
			elem.Left.Pixels = offset;
			Append(elem);
			offset += width + 10;
		};

		appendElement(new UIItemPanel(createItem, 50), 50);

		var conditionText = ConstraintText(null);

		_constraintText = new UIText(conditionText, 0.6f);
		_constraintText.Left.Pixels = offset;

		Append(_constraintText);

		var requiredItemsContainer = new UIAutoExtendGrid();
		requiredItemsContainer.Width = new StyleDimension(-60, 1);
		requiredItemsContainer.Top.Pixels = 20;
		requiredItemsContainer.HAlign = 1;

		foreach (var item in RequiredItems)
		{
			// See if there's a group in the recipe that accepts this item.
			var maybeGroup = AcceptedGroups
				.Select(g => {
					RecipeGroup.recipeGroups.TryGetValue(g, out var rg);
					return rg;
				})
			.FirstOrDefault(rg => rg?.ContainsItem(item.type) ?? false);

			var elem = maybeGroup == null
					? new UIItemPanel(item, 30)
					: new UIRecipeGroupPanel(maybeGroup, item.stack, 30);

			requiredItemsContainer.Append(elem);
		}

		Append(requiredItemsContainer);
	}

	public UIRecipePanel(Recipe recipe) :
		this(recipe.createItem, recipe.requiredItem, recipe.acceptedGroups, recipe.requiredTile,
			recipe.Conditions)
	{
	}

	public virtual void Highlight(IIngredient? source)
	{
		_constraintText?.SetText(ConstraintText(source));
	}

	private string ConstraintText(IIngredient? ingredient)
	{
		var conditionStrings =
		RequiredTiles.Select(t => HighlightedCraftingStationName(t, ingredient))
			.Concat(Conditions.Select(c => c.Description.Value));
		var conditionText = string.Join(", ", conditionStrings);
		return conditionText;
	}

	private static string HighlightedCraftingStationName(int tileID, IIngredient? ingredient)
	{
		var name = CraftingStationName(tileID);
		if (!QERConfig.Instance.HighlightClickedItems) { return name; }

		if (ingredient is ItemIngredient item && item.Item.createTile == tileID)
		{
			return $"[c/{Main.OurFavoriteColor.Hex3()}:{name}]";
		}

		return name;
	}

	private static string CraftingStationName(int tileID)
	{
		return tileID == -1
			? "?"
			: Lang.GetMapObjectName(MapHelper.TileToLookup(tileID, Recipe.GetRequiredTileStyle(tileID)));
	}
}
