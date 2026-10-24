using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using QuiteEnoughRecipes.ModUIElements;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.UI;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Terraria.UI.Chat;

namespace QuiteEnoughRecipes;

/*
 * Displays an NPC. Similar to a bestiary button, but is never locked. Shows the name of the NPC
 * when hovered.
 */
public class UINPCPanel : UIElement, IIngredientElement, IIngredientGridElement
{
	// This needs to be a child of the panel to handle overflow properly.
	private class UINPCIcon : UIElement, IIngredientElement, IHighlightableElement
	{
		private int _npcID = 0;
		private bool _isHovering => Parent?.IsMouseHovering ?? false;

		public bool IsHighlighted { get; protected set; }
		public IIngredient Ingredient => new NPCIngredient(NpcID);

		public BestiaryEntry Entry { get; private set; }

		private IEntryIcon _icon;
		// some npcs have entries that are removed from the bestiary so their Icon is null
		// this ensures that those entries are drawn anyway.
		public IEntryIcon Icon => _icon ??= (Entry.Icon ?? new UnlockableNPCEntryIcon(NpcID));
		public required int NpcID
		{
			get => _npcID;

			[MemberNotNull(nameof(Entry))]
			set
			{
				_npcID = value;
				Entry = Main.BestiaryDB.FindEntryByNPCID(_npcID);
				_icon = null!; // forces reinitialization
			}
		}

		public UINPCIcon()
		{
			OverflowHidden = true;
			IgnoresMouseInteraction = true;
			Width.Percent = 1;
			Height.Percent = 1;
		}

		public virtual void Highlight(IIngredient? source)
		{
			IsHighlighted = Ingredient is not null && source is not null && Ingredient.IsEquivalent(source);
		}

		public override void Update(GameTime t)
		{
			var rect = GetDimensions().ToRectangle();
			var collectionInfo = new BestiaryUICollectionInfo(){
				OwnerEntry = Entry,
				UnlockState = BestiaryEntryUnlockState.CanShowPortraitOnly_1
			};

			if (QuiteEnoughRecipes.LoadNPCAsync(NpcID).IsLoaded)
			{
				Icon.Update(collectionInfo, rect,
				new EntryIconDrawSettings()
				{
					iconbox = rect,
					IsHovered = _isHovering,
					IsPortrait = false
				});
			}
		}

		protected override void DrawSelf(SpriteBatch sb)
		{
			var collectionInfo = new BestiaryUICollectionInfo(){
				OwnerEntry = Entry,
				UnlockState = BestiaryEntryUnlockState.CanShowPortraitOnly_1
			};

			if (IsHighlighted)
			{
				var dim = GetDimensions().ToRectangle();
				
				dim.Inflate(-2, -2);
				sb.Draw(TextureAssets.MagicPixel.Value, dim, Main.OurFavoriteColor);
			}

			if (QuiteEnoughRecipes.LoadNPCAsync(NpcID).IsLoaded)
			{
				var dim = GetDimensions().ToRectangle();
				Icon.Draw(collectionInfo, sb,
				new EntryIconDrawSettings()
				{
					iconbox = dim,
					IsHovered = _isHovering,
					IsPortrait = false
				});
			}
		}
	}

	public const float DefaultSize = 72;

	private UINPCIcon _icon;
	private string _hoverText = "";

	public IIngredient Ingredient => new NPCIngredient(_icon.NpcID);

	public IIngredient Value => Ingredient;
	public UIElement Element => this;

	public UINPCPanel(int npcID, int size = 72)
	{
		_icon = new UINPCIcon{ NpcID = npcID};
		UpdateHoverText();

		Width.Pixels = Height.Pixels = size;
		OverflowHidden = true;

		var slotBack = new UIImage(Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Slot_Back")){
			Width = new StyleDimension(0, 1),
			Height = new StyleDimension(0, 1),
			HAlign = 0.5f,
			VAlign = 0.5f,
			ScaleToFit = true,
			IgnoresMouseInteraction = true
		};
		var slotFront = new UIImage(Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Slot_Front")){
			Width = new StyleDimension(0, 1),
			Height = new StyleDimension(0, 1),
			HAlign = 0.5f,
			VAlign = 0.5f,
			ScaleToFit = true,
			IgnoresMouseInteraction = true
		};

		Append(slotBack);
		Append(_icon);
		Append(slotFront);
	}

	public UINPCPanel() : this(0) {}

	public void SetDisplayedValue(NPCIngredient i)
	{
		_icon.NpcID = i.ID;
		UpdateHoverText();
	}

	protected override void DrawSelf(SpriteBatch sb)
	{
		base.DrawSelf(sb);

		if (IsMouseHovering)
		{
			UICommon.TooltipMouseText(_hoverText);
		}
	}

	private void UpdateHoverText()
	{
		ContentSamples.NpcsByNetId.TryGetValue(_icon.NpcID, out var npc);

		/*
		 * We color NPC names by rarity, similarly to items. Bosses use the same color as quest
		 * fish.
		 */
		int rarity = _icon.Entry.Info.Any(i => i is BossBestiaryInfoElement)
			? ItemRarityID.Quest
			: (npc?.rarity ?? 0);

		var rarityColor = ItemRarity.GetColor(rarity);
		var mod = npc?.ModNPC?.Mod;
		var modTag = mod == null ? "" : QuiteEnoughRecipes.GetModTagText(mod);

		_hoverText = $"[c/{rarityColor.Hex3()}:{Lang.GetNPCNameValue(_icon.NpcID)}]{modTag}";
		var flavorText = Ingredient.GetTooltipLines().FirstOrDefault();

		if (flavorText != null)
		{
			// Match width to the width of the name for long names.
			float width = ChatManager.GetStringSize(FontAssets.MouseText.Value, _hoverText,
				Vector2.One).X;
			width = MathF.Max(300, width);

			var wrappedFlavorText = FontAssets.MouseText.Value.CreateWrappedText(flavorText, width, Language.ActiveCulture.CultureInfo);
			_hoverText += $"\n{wrappedFlavorText}";
		}
	}

	public void SetDisplayedValue(IIngredient value)
	{
		if (value is NPCIngredient npcIng)
		{
			_icon.NpcID = npcIng.ID;
			UpdateHoverText();
		}
	}

	public void Configure(GridOptions options)
	{
		Width.Pixels = options.SideLength;
		Height.Pixels = options.SideLength;
	}

	public IGridElement<IIngredient> Copy()
	{
		return new UINPCPanel(_icon.NpcID);
	}
}
