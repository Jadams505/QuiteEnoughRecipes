using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using QuiteEnoughRecipes.ModIngredients;
using System;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Terraria.UI.Chat;

namespace QuiteEnoughRecipes.ModUIElements;
public class UIBiomePanel : UIElement, IIngredientElement, IScrollableGridElement<BiomeIngredient>, IHighlightableElement
{
	public static int GridSideLength { get; } = 52;
	public static int GridPadding { get; } = 5;
	
	public IFilterInfoProvider InfoId { get; set; }

	public string HoverText { get; protected set; } = "";
	public IIngredient Ingredient => new BiomeIngredient(InfoId);
	private UIElement? _iconElement = null;
	public bool IsHighlighted { get; protected set; }

	public UIBiomePanel(IFilterInfoProvider infoId, int size = 50)
	{
		InfoId = infoId;
		UpdateHoverText();

		Width.Pixels = size;
		Height.Pixels = size;

		SetIconElement(InfoId);
	}

	public UIBiomePanel() : this(new DefaultBiomeInfo())
	{

	}

	private void SetIconElement(IFilterInfoProvider info)
	{
		if (_iconElement is not null)
			RemoveChild(_iconElement);

		var image = InfoId.GetFilterImage();

		// this is need for nested elements since it is not an IIngredientElement
		// which is a required check for usage/source clicks.
		image.IgnoresMouseInteraction = true;

		_iconElement = image;
		Append(_iconElement);
	}

	public void SetDisplayedValue(BiomeIngredient ing)
	{
		InfoId = ing.InfoType;

		SetIconElement(InfoId);
		
		UpdateHoverText();
	}

	public virtual void Highlight(IIngredient? source)
	{
		IsHighlighted = Ingredient is not null && source is not null && Ingredient.IsEquivalent(source);
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		base.DrawSelf(spriteBatch);

		var pos = GetDimensions().Position();

		var inventoryBack = IsHighlighted
			? TextureAssets.InventoryBack14.Value
			: TextureAssets.InventoryBack.Value;

		var scale = GetDimensions().Width / GridSideLength;
		spriteBatch.Draw(inventoryBack, pos, null, Color.White, 0, Vector2.Zero, scale, 0, 0);

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
			width = MathF.Max(300, width);

			var wrappedFlavorText = FontAssets.MouseText.Value.CreateWrappedText(line, width);
			HoverText += $"\n{wrappedFlavorText}";
		}
	}

	private class DefaultBiomeInfo() : IFilterInfoProvider
	{
		public string GetDisplayNameKey() => "DefaultBiomeInfo";

		public UIElement GetFilterImage() => new UIElement();
	}
}
