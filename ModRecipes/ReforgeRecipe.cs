using QuiteEnoughRecipes.ModUIElements;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.UI;

namespace QuiteEnoughRecipes.ModRecipes;

internal class ReforgeRecipe : IRecipe
{
    public required Item PrefixedItem;

    [SetsRequiredMembers]
    public ReforgeRecipe(Item prefixedItem)
    {
        PrefixedItem = prefixedItem;
    }

    public UIElement Element => new UIReforgePanel(PrefixedItem);

    public IEnumerable<IIngredient> GetIngredients()
    {
        yield return new ItemIngredient(PrefixedItem);
    }
}
