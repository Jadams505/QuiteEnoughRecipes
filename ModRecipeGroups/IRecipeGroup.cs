using System.Collections.Generic;

namespace QuiteEnoughRecipes.ModRecipeGroups;

public interface IRecipeGroup
{
	public string GetText();
	public HashSet<int> ValidItems();
	public int IconItem { get; }
}