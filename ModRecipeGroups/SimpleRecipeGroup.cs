using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;

namespace QuiteEnoughRecipes.ModRecipeGroups;

public record SimpleRecipeGroup(Func<string> NameFunc, HashSet<int> AcceptedItems, int IconItem) : IRecipeGroup
{
	public SimpleRecipeGroup(Func<string> NameFunc, HashSet<int> AcceptedItems) : this(NameFunc, AcceptedItems, 0)
	{
		IconItem = AcceptedItems.FirstOrDefault(0);
	}

	public string GetText() => NameFunc();

	public HashSet<int> ValidItems() => AcceptedItems;
}

public static class RecipeGroupExtensions
{
	public static SimpleRecipeGroup ToSimpleRecipeGroup(this RecipeGroup group)
	{
		return new(group.GetText, group.ValidItems, group.GetPlaceholderItemType());
	} 
}