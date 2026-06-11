using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using QuiteEnoughRecipes.ModIngredients;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Terraria.UI.Chat;

namespace QuiteEnoughRecipes.ModUIElements;
public class UIBuffPanel : UIElement, IIngredientElement, IScrollableGridElement<BuffIngredient>, IHighlightableElement
{
	public static int GridSideLength { get; } = 52;
	public static int GridPadding { get; } = 5;
	
	public int BuffId { get; set; }

	public int Border { get; set; }
	public string HoverText { get; protected set; } = "";
	public IIngredient Ingredient => new BuffIngredient(BuffId);
	public bool IsHighlighted { get; protected set; }

	public UIBuffPanel(int buffId, int size = 50)
	{
		BuffId = buffId;
		UpdateHoverText();
		Border = 16;

		Width.Pixels = size;
		Height.Pixels = size;
	}

	public UIBuffPanel() : this(1)
	{

	}

	public void SetDisplayedValue(BuffIngredient ing)
	{
		BuffId = ing.BuffType;
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

		DrawBuff(spriteBatch);

		if (IsMouseHovering)
		{
			UICommon.TooltipMouseText(HoverText);
		}
	}

	private void DrawBuff(SpriteBatch spriteBatch)
	{
		if (BuffId <= 0 || BuffId >= BuffLoader.BuffCount)
			return;

		var asset = QuiteEnoughRecipes.LoadBuffAsync(BuffId);
		if (!asset.IsLoaded)
			return;

		var dimensions = GetInnerDimensions();
		var size = asset.Size();
		float drawScale = Math.Min((dimensions.Width - Border) / size.X, (dimensions.Height - Border) / size.Y);
		spriteBatch.Draw(asset.Value, dimensions.Center(), new(0, 0, (int)size.X, (int)size.Y), Color.White, 0f, size * drawScale / 2f, drawScale, SpriteEffects.None, 0f);
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
}
