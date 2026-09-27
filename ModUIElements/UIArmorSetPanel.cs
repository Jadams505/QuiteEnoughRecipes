using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using QuiteEnoughRecipes.ModIngredients;
using System;
using System.Diagnostics.CodeAnalysis;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Terraria.UI.Chat;

namespace QuiteEnoughRecipes.ModUIElements;

public class UIArmorSetPanel : UIElement, IIngredientElement, IScrollableGridElement<ArmorSetIngredient>, IHighlightableElement
{
	public static int GridSideLength { get; } = 52;
	public static int GridPadding { get; } = 5;

	private Player _fakePlayer;
	private Item[] _armor = new Item[3];
	public required ArmorSetBonus ArmorSet { get; set; }

	public string HoverText { get; protected set; } = "";
	public IIngredient Ingredient => new ArmorSetIngredient(ArmorSet);
	public bool IsHighlighted { get; protected set; }

	[SetsRequiredMembers]
	public UIArmorSetPanel(ArmorSetBonus armorSet, int size = 72)
	{
		_fakePlayer = new Player();
		ArmorSet = armorSet;
		_armor[0] = new(ArmorSet.Head);
		_armor[1] = new(ArmorSet.Body);
		_armor[2] = new(ArmorSet.Legs);

		UpdateHoverText();

		Width.Pixels = 72;
		Height.Pixels = 72;

	}

	[SetsRequiredMembers]
	public UIArmorSetPanel() : this(new EmptyArmorSetBonus())
	{

	}

	public void SetDisplayedValue(ArmorSetIngredient ing)
	{
		ArmorSet = ing.ArmorSet;
		UpdateHoverText();
	}

	public virtual void Highlight(IIngredient? source)
	{
		IsHighlighted = Ingredient is not null && source is not null && Ingredient.IsEquivalent(source);
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		base.DrawSelf(spriteBatch);

		var dim = GetDimensions();
		var pos = dim.Position();

		var inventoryBack = IsHighlighted
			? TextureAssets.InventoryBack14.Value
			: TextureAssets.InventoryBack.Value;

		var scale = GetDimensions().Width / GridSideLength;

		spriteBatch.Draw(inventoryBack, pos, null, Color.White, 0, Vector2.Zero, scale, 0, 0);

		var player = _fakePlayer;
		for (int i = 0; i < _armor.Length; ++i)
		{
			player.armor[i] = _armor[i];
		}
		player.ResetEffects();
		player.ResetVisibleAccessories();
		player.UpdateMiscCounter();
		player.UpdateDyes();

		player.PlayerFrame();
		player.socialIgnoreLight = true; // this makes sure players are drawn at max light.

		var playerSize = player.Size;
		float playerDrawScale = GetDimensions().Width / 72f; // 72 with a 1f scale seems to fit well
		Main.PlayerRenderer.DrawPlayer(Main.Camera, player, pos 
			+ Main.screenPosition 
			+ player.position 
			+ new Vector2(dim.Width / 2f, dim.Height / 2f) 
			- new Vector2(playerSize.X / 2f, playerSize.Y / 2f), 
			0f, Vector2.Zero, 0f, playerDrawScale);
		UseImmediateMode = true; // this is needed to make the player draw in the correct order

		if (IsMouseHovering)
		{
			UICommon.TooltipMouseText(HoverText);
		}
	}

	private void UpdateHoverText()
	{
		var mod = Ingredient.Mod;
		var modTag = mod is null ? "" : QuiteEnoughRecipes.GetModTagText(mod);

		HoverText = $"{Ingredient.Name}{modTag}";
		var flavorText = Ingredient.GetTooltipLines();

		foreach (var line in flavorText)
		{
			// Match width to the width of the name for long names.
			float width = ChatManager.GetStringSize(FontAssets.MouseText.Value, HoverText,
				Vector2.One).X;
			// set bonuses are long strings, so doubled max width
			width = MathF.Max(600, width);

			var wrappedFlavorText = FontAssets.MouseText.Value.CreateWrappedText(line, width, Language.ActiveCulture.CultureInfo);
			HoverText += $"\n{wrappedFlavorText}";
		}
	}
}

public class EmptyArmorSetBonus : ArmorSetBonus
{
	public EmptyArmorSetBonus()
	{
		Effect = (p) => { };
		Description = LocalizedText.Empty;
	}
}
