using QuiteEnoughRecipes.ModFiltersHandlers;
using System.Collections.Generic;
using Terraria.Localization;
using Terraria.ModLoader;

namespace QuiteEnoughRecipes.ModAPI;

public abstract class QERFilterHandler : ModType, IFilterHandler<IIngredient>
{
	public int Type { get; internal set; }

	public abstract LocalizedText FilterGroupText { get; }

	public abstract bool Applies(IEnumerable<IIngredient> items);

	public abstract IEnumerable<IFilterTab<IIngredient>> GetFilters();

	/// <summary>
	/// By default icons are sorted in load order.<br/>
	/// This hook allows you to sort this icon before/after other icons.<br/>
	/// Example:
	/// <para>
	/// <c>yield return new After(ModContent.GetInstance&lt;RecipeHandlers.CraftingStations&gt;());</c>
	/// </para>
	/// By default, this hook returns <c>[]</c>, which indicates that this icon has no ordering constraints.
	/// </summary>
	public virtual IEnumerable<Position> GetConstraints() => [];

	protected sealed override void Register()
	{
		ModTypeLookup<QERFilterHandler>.Register(this);
		Type = FilterHandlerLoader.Add(this);
	}

	public sealed override void SetupContent() => SetStaticDefaults();

	public abstract class Position;

	public sealed class Before : Position
	{
		public QERFilterHandler Value { get; }
		public Before(QERFilterHandler value)
		{
			Value = value;
		}

		public static Before Instance<T>() where T : QERFilterHandler
		{
			return new(ModContent.GetInstance<T>());
		}
	}

	public sealed class After : Position
	{
		public QERFilterHandler Value { get; }
		public After(QERFilterHandler value)
		{
			Value = value;
		}

		public static After Instance<T>() where T : QERFilterHandler
		{
			return new(ModContent.GetInstance<T>());
		}
	}
}
