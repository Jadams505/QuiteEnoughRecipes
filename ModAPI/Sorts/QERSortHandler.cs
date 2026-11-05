using QuiteEnoughRecipes.ModFiltersHandlers;
using System.Collections.Generic;
using Terraria.Localization;
using Terraria.ModLoader;

namespace QuiteEnoughRecipes.ModAPI.Sorts;

public abstract class QERSortHandler : ModType, ISortHandler<IIngredient>
{
	public int Type { get; internal set; }

	public abstract LocalizedText SortGroupText { get; }

	public abstract bool Applies(IEnumerable<IIngredient> items);

	public abstract IEnumerable<ISortTab<IIngredient>> GetSorts();

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
		ModTypeLookup<QERSortHandler>.Register(this);
		Type = SortHandlerLoader.Add(this);
	}

	public sealed override void SetupContent() => SetStaticDefaults();

	public abstract class Position;

	public sealed class Before : Position
	{
		public QERSortHandler Value { get; }
		public Before(QERSortHandler value)
		{
			Value = value;
		}

		public static Before Instance<T>() where T : QERSortHandler
		{
			return new(ModContent.GetInstance<T>());
		}
	}

	public sealed class After : Position
	{
		public QERSortHandler Value { get; }
		public After(QERSortHandler value)
		{
			Value = value;
		}

		public static After Instance<T>() where T : QERSortHandler
		{
			return new(ModContent.GetInstance<T>());
		}
	}
}
