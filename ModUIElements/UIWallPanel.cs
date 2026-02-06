using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using QuiteEnoughRecipes.ModIngredients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Terraria.UI.Chat;

namespace QuiteEnoughRecipes.ModUIElements;

public class UIWallPanel : UIElement, IIngredientElement, IScrollableGridElement<WallIngredient>, IHighlightableElement
{
	public static int GridSideLength { get; } = 52;
	public static int GridPadding { get; } = 5;

	public int WallId { get; set; }

	public int Border { get; set; }
	public string HoverText { get; protected set; } = "";
	public IIngredient Ingredient => new WallIngredient(WallId);
	public bool IsHighlighted { get; protected set; }

	public UIWallPanel(int wallId, int size = 50)
	{
		WallId = wallId;
		UpdateHoverText();
		Border = 16;

		Width.Pixels = size;
		Height.Pixels = size;
	}

	public UIWallPanel() : this(0)
	{

	}

	public void SetDisplayedValue(WallIngredient ing)
	{
		WallId = ing.WallType;
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

		DrawWall(spriteBatch);

		if (IsMouseHovering)
		{
			UICommon.TooltipMouseText(HoverText);
		}
	}

	private void DrawWall(SpriteBatch spriteBatch)
	{
		if (WallId > WallID.None && WallId < WallLoader.WallCount)
		{
			QuiteEnoughRecipes.LoadWallAsync(WallId);

			var startPos = new Point(324, 108);
			DrawWall(spriteBatch, startPos);
		}
	}

	private void DrawWall(SpriteBatch spriteBatch, Point startPos)
	{
		var texture = TextureAssets.Wall[WallId];
		var dimensions = GetInnerDimensions();

		// Fixes blurry textures
		RasterizerState rasterizerState = spriteBatch.GraphicsDevice.RasterizerState;
		spriteBatch.End();
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, rasterizerState, null, Main.UIScaleMatrix);

		var size = 32;
		float drawScale = Math.Min((dimensions.Width - Border) / size, (dimensions.Height - Border) / size);

		spriteBatch.Draw(texture.Value, dimensions.Center(), new(startPos.X, startPos.Y, size, size), Color.White, 0f, Vector2.One * (size / 2), drawScale, SpriteEffects.None, 0);
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
