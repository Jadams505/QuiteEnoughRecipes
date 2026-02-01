using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.UI.Elements;

namespace QuiteEnoughRecipes.ModUIElements;

public delegate void HighlightAction(IIngredient? source);

public class UIHighlightableText : UIText, IHighlightableElement
{
    public event HighlightAction? DoHighlight;

    public UIHighlightableText(string text, float textScale = 1, bool large = false) : base(text, textScale, large)
    {

    }

    public virtual void Highlight(IIngredient? source)
    {
        if (DoHighlight is not null)
            DoHighlight(source);
    }
}
