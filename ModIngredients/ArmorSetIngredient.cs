using System;
using System.Collections.Generic;
using System.Text;
using Terraria.DataStructures;
using Terraria.GameContent.UI.Chat;
using Terraria.ModLoader;

namespace QuiteEnoughRecipes.ModIngredients;

public record struct ArmorSetIngredient(ArmorSetBonus ArmorSet) : IIngredient
{
	// There is realy not good way to tell the name of a set of armor.
	// Could potentially use armor piece names, but might be too long
	public string Name => "Armor Set"; 

	public IEnumerable<string> GetTooltipLines()
	{
		if (ArmorSet.Effect?.Method?.Name is string effect)
			yield return $"Effect: {effect}";
		yield return $"[c/828282:Set Bonus: {ArmorSet.Description.Value}]";
	}

	public bool IsEquivalent(IIngredient other)
	{
		if (other is not ArmorSetIngredient armorIng) return false;

		return ArmorSet.Head == armorIng.ArmorSet.Head &&
			ArmorSet.Body == armorIng.ArmorSet.Body &&
			ArmorSet.Legs == armorIng.ArmorSet.Legs &&
			ArmorSet.PrimaryPart == armorIng.ArmorSet.PrimaryPart;
	}
}
