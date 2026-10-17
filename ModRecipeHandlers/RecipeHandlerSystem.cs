using QuiteEnoughRecipes.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using Terraria.ModLoader;

namespace QuiteEnoughRecipes.ModRecipeHandlers;

public class RecipeHandlerSystem : ModSystem
{
	public static RecipeHandlerSystem Instance => field ??= ModContent.GetInstance<RecipeHandlerSystem>();

	public override void ResizeArrays()
	{
		RecipeHandlerLoader.ResizeArrays();
	}
}
