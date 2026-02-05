using System.Collections.Generic;
using System.Linq;
using System;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria;
using Terraria.ID;
using QuiteEnoughRecipes.ModIngredients;
using Terraria.ObjectData;
using System.Runtime.CompilerServices;
using QuiteEnoughRecipes.ModRecipeHandlers;

namespace QuiteEnoughRecipes;

/*
 * Used as a general hub for information about ingredients, including full ingredient lists, sorts,
 * and filters.
 */
public class IngredientRegistry : ModSystem
{
	// Maybe there's a built-in interface for this, but I don't know what it is.
	private interface IGenericValue<T>
	{
		public T Value { get; }
	}

	private abstract class AbstractGenericOption<T> : IGenericValue<T>
	{
		public abstract T Value { get; }

		// For convenience. The specific ingredient type associated with this option.
		public abstract Type IngredientType { get; }

		public required LocalizedText Name;
		public required Func<UIElement> ElementFunc;

		/*
		 * Localization key for the header above this option. Options with the same key will be
		 * grouped in the same section on the option panel.
		 */
		public string SectionNameKey = "";
	}

	private class IngredientFilter<T> : AbstractGenericOption<Predicate<IIngredient>>
		where T : IIngredient
	{
		public required Predicate<T> Predicate;
		public override Predicate<IIngredient> Value => i => i is T t && Predicate(t);
		public override Type IngredientType => typeof(T);
	}

	private class IngredientSort<T> : AbstractGenericOption<Comparison<IIngredient>>
		where T : IIngredient
	{
		public required Comparison<T> Comparison;
		public override Comparison<IIngredient> Value => (x, y) => {
			if (!(x is T xt && y is T yt))
			{
				return x.GetType().Name.CompareTo(y.GetType().Name);
			}

			return Comparison(xt, yt);
		};
		public override Type IngredientType => typeof(T);
	}

	/*
	 * Normally, it's perfectly possible to convert a `List<T>` to an `IEnumerable<U>` if `T`
	 * derives from `U`. However, this covariance only works if `T` is a class type and not a
	 * struct type. As of now, I have all `IIngredient`s as structs, so they don't work. There's
	 * no good reason not to change them to classes other than my own stubbornness, but this is my
	 * workaround.
	 */
	private class IngredientList<T> : List<T>, IGenericValue<IEnumerable<IIngredient>>
		where T : IIngredient
	{
		public IngredientList(IEnumerable<T> values) : base(values) {}
		public IEnumerable<IIngredient> Value => this.Select(i => i as IIngredient);
	}

	private List<AbstractGenericOption<Predicate<IIngredient>>> _filters = [];
	private List<AbstractGenericOption<Comparison<IIngredient>>> _sorts = [];

	// Map ingredient types to master lists of ingredients of that type.
	private Dictionary<Type, IGenericValue<IEnumerable<IIngredient>>> _ingredients = new();

	public static IngredientRegistry Instance => ModContent.GetInstance<IngredientRegistry>();

	/*
	 * Reset to the default sorts and filters. This is currently done during `PostSetupRecipes` to
	 * ensure that all content from other mods can be properly loaded. However, This should be
	 * designed in such a way that there's a clear time where other mods can add custom filters and
	 * stuff.
	 *
	 * TODO: Is `PostSetupRecipes` the correct option here?
	 */
	public override void PostSetupRecipes()
	{
		_filters.Clear();
		_sorts.Clear();
		_ingredients.Clear();

		var allItems = Enumerable.Range(0, ItemLoader.ItemCount)
			.Select(i => new Item(i))
			.Where(i => i.type != 0)
			.Select(i => new ItemIngredient(i));
		AddIngredients(allItems);

		var allNPCs = Enumerable.Range(0, NPCLoader.NPCCount)
			.Where(n => Main.BestiaryDB.FindEntryByNPCID(n).Icon != null)
			.Select(n => new NPCIngredient(n))
			.ToList();
		AddIngredients(allNPCs);

		var allTiles = Enumerable.Range(0, TileLoader.TileCount)
			.SelectMany(TileToIngredients)
			.ToList();
		AddIngredients(allTiles);

		var allWalls = Enumerable.Range(0, WallLoader.WallCount)
			.Select(w => new WallIngredient(w))
			.ToList();
		AddIngredients(allWalls);

		var keyParent = "Mods.QuiteEnoughRecipes.OptionGroups";

		var miscItemFilters = IngredientOptions.GetOptionButtons<Predicate<ItemIngredient>>(
			"ItemFilters.Misc");
		foreach (var (pred, icon, name) in miscItemFilters)
		{
			AddFilter<ItemIngredient>(pred, icon, name, $"{keyParent}.ItemFilters.Misc.Name");
		}

		var weaponItemFilters = IngredientOptions.GetOptionButtons<Predicate<ItemIngredient>>(
			"ItemFilters.Weapons");
		foreach (var (pred, icon, name) in weaponItemFilters)
		{
			AddFilter<ItemIngredient>(pred, icon, name, $"{keyParent}.ItemFilters.Weapons.Name");
		}

		var npcFilters = IngredientOptions.GetOptionButtons<Predicate<NPCIngredient>>("NPCFilters");
		foreach (var (pred, icon, name) in npcFilters)
		{
			AddFilter<NPCIngredient>(pred, icon, name, $"{keyParent}.NPCFilters.Name");
		}

		var tileFilters = IngredientOptions.GetOptionButtons<Predicate<TileIngredient>>(IngredientOptions.TileFiltersKey);
		foreach (var (pred, icon, name) in tileFilters)
		{
			AddFilter<TileIngredient>(pred, icon, name, $"{keyParent}.TileFilters.Name");
		}

		var wallFilters = IngredientOptions.GetOptionButtons<Predicate<WallIngredient>>(IngredientOptions.WallFiltersKey);
		foreach (var (pred, icon, name) in wallFilters)
		{
			AddFilter<WallIngredient>(pred, icon, name, $"{keyParent}.{IngredientOptions.WallFiltersKey}.Name");
		}

		var itemSorts = IngredientOptions.GetOptionButtons<Comparison<ItemIngredient>>(
			"ItemSorts");
		foreach (var (comp, icon, name) in itemSorts)
		{
			AddSort<ItemIngredient>(comp, icon, name, $"{keyParent}.ItemSorts.Name");
		}

		var npcSorts = IngredientOptions.GetOptionButtons<Comparison<NPCIngredient>>("NPCSorts");
		foreach (var (comp, icon, name) in npcSorts)
		{
			AddSort<NPCIngredient>(comp, icon, name, $"{keyParent}.NPCSorts.Name");
		}

		var tileSorts = IngredientOptions.GetOptionButtons<Comparison<TileIngredient>>("TileSorts");
		foreach (var (comp, icon, name) in tileSorts)
		{
			AddSort<TileIngredient>(comp, icon, name, $"{keyParent}.TileSorts.Name");
		}

		var wallSorts = IngredientOptions.GetOptionButtons<Comparison<WallIngredient>>("WallSorts");
		foreach (var (comp, icon, name) in wallSorts)
		{
			AddSort<WallIngredient>(comp, icon, name, $"{keyParent}.WallSorts.Name");
		}
	}

	public void AddFilter<T>(Predicate<T> pred, int iconItemID, LocalizedText name,
		string sectionNameKey = "") where T : IIngredient
	{
		_filters.Add(new IngredientFilter<T>{
			Predicate = pred,
			Name = name,
			ElementFunc = () => new UIItemIcon(new(iconItemID), false),
			SectionNameKey = sectionNameKey,
		});
	}

	public void AddSort<T>(Comparison<T> comp, int iconItemID, LocalizedText name,
		string sectionNameKey = "") where T : IIngredient
	{
		// The first added sort is automatically the default.
		bool shouldBeDefault = !_sorts.Any(s => s is IngredientSort<T>);

		_sorts.Add(new IngredientSort<T>{
			Comparison = comp,
			Name = name,
			ElementFunc = () => new UIItemIcon(new(iconItemID), false),
			SectionNameKey = sectionNameKey,
		});
	}

	// Add ingredients to the "master list" for a given type.
	public void AddIngredients<T>(IEnumerable<T> ingredients)
		where T : IIngredient
	{
		if (_ingredients.TryGetValue(typeof(T), out var list))
		{
			(list as IngredientList<T>)?.AddRange(ingredients);
		}
		else
		{
			list = new IngredientList<T>(ingredients);
			_ingredients.Add(typeof(T), list);
		}
	}

	/*
	 * If no ingredient list exists for `T`, an empty list will be returned. Otherwise, this is the
	 * master list of ingredients of type `T`.
	 */
	public List<T> GetIngredients<T>() where T : IIngredient
	{
		_ingredients.TryGetValue(typeof(T), out var list);
		return list as List<T> ?? [];
	}

	// Make a filter group element with the filters for type `T`.
	public UIFilterGroup<T> MakeFilterGroup<T>()
		where T : IIngredient
	{
		var group = new UIFilterGroup<T>();
		foreach (var opt in _filters.OfType<IngredientFilter<T>>())
		{
			group.AddFilter(opt.Predicate, opt.Name, opt.ElementFunc(), opt.SectionNameKey);
		}

		return group;
	}

	// Get the generic filter group for ingredient type `t`.
	public UIFilterGroup<IIngredient> MakeFilterGroup(Type t)
	{
		var group = new UIFilterGroup<IIngredient>();
		foreach (var opt in _filters.Where(f => f.IngredientType == t))
		{
			group.AddFilter(opt.Value, opt.Name, opt.ElementFunc(), opt.SectionNameKey);
		}

		return group;
	}

	/*
	 * Make a filter group for filtering by mod. Only mods included in the list of ingredients of
	 * type `T` will be used.
	 */
	public UIFilterGroup<T> MakeModFilterGroup<T>()
		where T : IIngredient
	{
		_ingredients.TryGetValue(typeof(T), out var list);
		var mods = (list?.Value ?? []).Select(i => i.Mod)
			.Where(m => m is not null)
			.Select(m => m!);
		return MakeModFilterGroup<T>(mods);
	}

	// Make a generic mod filter group for ingredients of any type in `ingredientTypes`.
	public UIFilterGroup<IIngredient> MakeModFilterGroup(
		IEnumerable<Type> ingredientTypes)
	{
		var mods = ingredientTypes
			.SelectMany(t => _ingredients.TryGetValue(t, out var ings) ? ings.Value : [])
			.Select(i => i.Mod)
			.Where(m => m is not null)
			.Select(m => m!);
		return MakeModFilterGroup<IIngredient>(mods);
	}

	/*
	 * Make a filter group containing each mod in `mods`. Only one filter will be added for each
	 * different mod in `mods`, and they will be in alphabetical order.
	 */
	public UIFilterGroup<T> MakeModFilterGroup<T>(IEnumerable<Mod> mods)
		where T : IIngredient
	{
		var keyParent = "Mods.QuiteEnoughRecipes.OptionGroups.Mods";
		var group = new UIFilterGroup<T>();

		var sortedMods = mods.Distinct().OrderBy(m => m.DisplayNameClean);
		foreach (var mod in sortedMods)
		{
			var name = Language.GetText($"{keyParent}.ModName")
				.WithFormatArgs(mod.DisplayNameClean);
			var icon = mod.ModSourceBestiaryInfoElement.GetFilterImage();
			group.AddFilter(i => i.Mod == mod, name, icon, $"{keyParent}.Name");
		}

		return group;
	}

	// Make a sort group element with the sort comparisons for type `T`.
	public UISortGroup<T> MakeSortGroup<T>() where T : IIngredient
	{
		var opts = _sorts.OfType<IngredientSort<T>>();
		return DoMakeSortGroup<T, IngredientSort<T>>(opts, s => s.Comparison);
	}

	/*
	 * Create a single sort group for all types in `types`. Unlike filters, it really only makes
	 * sense to have one sort active at a time, so they should all be packed into one group.
	 */
	public UISortGroup<IIngredient> MakeSortGroup(IEnumerable<Type> types)
	{
		var typesList = types.ToList();
		var opts = _sorts.Where(f => typesList.Any(t => f.IngredientType == t));
		return DoMakeSortGroup<
			IIngredient,
			AbstractGenericOption<Comparison<IIngredient>>>(opts, s => s.Value);
	}

	private static UISortGroup<T> DoMakeSortGroup<T, U>(IEnumerable<U> opts,
		Func<U, Comparison<T>> getValue) where U : AbstractGenericOption<Comparison<IIngredient>>
	{
		var optEnum = opts.GetEnumerator();

		// If there are no options, then we have to resort to a fake sort that does nothing.
		UISortGroup<T> group;
		if (!optEnum.MoveNext())
		{
			group = new UISortGroup<T>((x, y) => 0, Language.GetText(""),
				new UIItemIcon(new(ItemID.AngelStatue), false), "");
		}
		else
		{
			var opt = optEnum.Current;
			group = new UISortGroup<T>(getValue(opt), opt.Name, opt.ElementFunc(),
				opt.SectionNameKey);
		}

		while (optEnum.MoveNext())
		{
			var opt = optEnum.Current;
			group.AddSort(getValue(opt), opt.Name, opt.ElementFunc(), opt.SectionNameKey);
		}

		return group;
	}

	[UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_SubTiles")]
	internal static extern List<TileObjectData> TileObjectData_SubTiles(TileObjectData self);

	private static IEnumerable<TileIngredient> TileToIngredients(int tileId)
	{
		if (Main.tileFrameImportant[tileId])
		{
			var ingredientSet = new HashSet<TileIngredient>();
			if (TileObjectData.GetTileData(tileId, 0) is var data and not null && TileObjectData_SubTiles(data) is var subtiles and not null)
			{
				for (int i = 1; i < subtiles.Count; ++i)
				{
					ingredientSet.Add(new TileIngredient(tileId, i));
				}
			}

			foreach (var entry in TileDropsHelper.TileTypeAndTileStyleToItemType)
			{
				// -1 means all styles, which isn't useful in trying to enumerate all styles
				if (entry.Key.Style == -1) continue;
				if (entry.Key.TileId != tileId) continue;

				ingredientSet.Add(new TileIngredient(entry.Key.TileId, entry.Key.Style));
			}
			ingredientSet.Add(new TileIngredient(tileId, 0));

			foreach (var entry in ingredientSet)
				yield return entry;
		}
		else
		{
			yield return new TileIngredient(tileId);
		}
	}
}
