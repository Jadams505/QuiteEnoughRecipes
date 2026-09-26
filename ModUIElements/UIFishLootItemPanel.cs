using QuiteEnoughRecipes.ModRecipeGroups;
using QuiteEnoughRecipes.ModRecipeHandlers;
using System;
using System.Collections.Generic;
using System.Text;
using Terraria;
using Terraria.GameContent.FishDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace QuiteEnoughRecipes.ModUIElements;

public class UIFishLootItemPanel : UIItemPanel
{
	private readonly FishDropRule _rule;
	public FishDropRule Rule => _rule;
	private string? _conditions = null;
	private string? _rarity = null;

	public UIFishLootItemPanel(FishDropRule rule, int item) : base(new Item(item))
	{
		_rule = rule;

		_conditions = ConditionDesc(rule.Conditions);
		_rarity = RarityDesc(rule.Rarity);
	}

	public override void ModifyTooltips(Mod mod, List<TooltipLine> tooltips)
	{
		base.ModifyTooltips(mod, tooltips);
		
		// Not 100% sure what these numbers mean, looks like it is more complicated than catch chance
		var percent = FormatDropChance((float)Rule.ChanceNumerator / Rule.ChanceDenominator);
		if (percent != "")
		{
			var line = Language.GetText("Mods.QuiteEnoughRecipes.Tooltips.DropChance")
				.Format(percent);
			tooltips.Add(new(mod, "QER: drop chance", line)
			{
				Color = Main.OurFavoriteColor
			});
		}

		if (!string.IsNullOrWhiteSpace(_conditions))
		{
			tooltips.Add(new(mod, "QER: drop conditions", _conditions)
			{
				Color = Main.OurFavoriteColor
			});
		}

		if (!string.IsNullOrWhiteSpace(_rarity))
		{
			tooltips.Add(new(mod, "QER: rarity conditions", _rarity)
			{
				Color = Main.OurFavoriteColor
			});
		}
	}

	private static string ConditionDesc(AFishingCondition[] conditions)
	{
		var descriptions = new List<string>();
		int unknownConditions = 0;

		foreach (AFishingCondition condition in conditions)
		{
			if (condition is QERFishDropRulesPopulator.QERFishingCondition QERCond)
			{
				if (!string.IsNullOrEmpty(QERCond.Description.Value))
					descriptions.Add(QERCond.Description.Value);
			}
			else
			{
				unknownConditions++;
			}
		}

		if (unknownConditions > 0)
		{
			var line = Language.GetText("Mods.QuiteEnoughRecipes.Tooltips.OtherConditions")
				.Format(unknownConditions);
			descriptions.Add(line);
		}

		return string.Join("\n", descriptions);
	}

	private static string RarityDesc(FishRarityCondition condition)
	{
		var builder = new StringBuilder();

		if (condition is QERFishDropRulesPopulator.QERFishingRarity QERRarity)
		{
			var line = Language.GetText("Mods.QuiteEnoughRecipes.Tooltips.Rarity")
				.Format(QERRarity.Description.Value);
			builder.Append(line);
		}
		return builder.ToString();
	}

	private static string FormatDropChance(float chance)
	{
		if (chance < 0.00001f)
		{
			return $"<{0.00001f:p3}";
		}
		else if (chance < 0.0001f)
		{
			return $"{chance:p3}";
		}
		else if (chance < 0.9999f)
		{
			return $"{chance:p2}";
		}
		else
		{
			return "";
		}
	}
}

public class UIFishConditionPanel : UIRecipeGroupPanel
{
	private readonly AFishingCondition _condition;
	public AFishingCondition Condition => _condition;

	private string _text;
	public UIFishConditionPanel(AFishingCondition condition) : base(RecipeGroupSystem.AllFishingRods)
	{
		_condition = condition;
		_text = ToText(condition);
	}

	public override void ModifyTooltips(Mod mod, List<TooltipLine> tooltips)
	{
		base.ModifyTooltips(mod, tooltips);

		tooltips.Add(new(mod, "QER: fish conditions", _text)
		{
			Color = Main.OurFavoriteColor
		});
	}

	private static string ToText(AFishingCondition cond)
	{
		if (cond is QERFishDropRulesPopulator.QERFishingCondition QERCond)
		{
			return QERCond.Description.Value;
		}
		else
		{
			return cond.ToString() ?? Language.GetTextValue("Mods.QuiteEnoughRecipes.Conditions.Unknown");
		}
	}
}
