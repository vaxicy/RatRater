using System.Collections.Generic;
using UnityEngine;

// 直接引用游戏类型（不使用运行时反射，避免 reflection-external 能力声明）。
internal static class GameBridge
{
    public static void Init() { }

    public static IEnumerable<GameItem> GetAllElements()
    {
        return EmporiumEntry.Instance.GetAllOwnedItems(false, true);
    }

    public static GameItemElement AsElement(GameItem item) { return item as GameItemElement; }

    public static RectTransform GetRectTransform(GameItemElement element)
    {
        // 用 background 节点：它挂在 rotator 下，会跟随老鼠的 90° 旋转，
        // 竖放的老鼠高亮才是竖的（element.rectTransform 是未旋转的外框）。
        return element.background != null ? element.background.rectTransform : element.rectTransform;
    }

    public static bool IsRat(GameItem item)
    {
        return HusbandryHelper.GetSpecies(item) == AnimalSpecies.Rat ||
               item.IsTag(HusbandryHelper.ANIMAL_ITEM_TAG);
    }

    public static bool IsDead(GameItem item)
    {
        return HusbandryHelper.IsAnimalDead(item);
    }

    public static bool HasTag(GameItem item, string tag)
    {
        return item.IsTag(tag);
    }

    public static float GetFloatTag(GameItem item, string tag)
    {
        if (string.IsNullOrEmpty(tag) || !item.IsTag(tag)) return 0f;
        return item.GetTagReadonly(tag).GetFloat();
    }

    public static string GetStringTag(GameItem item, string tag)
    {
        if (string.IsNullOrEmpty(tag) || !item.IsTag(tag)) return null;
        return item.GetTagReadonly(tag).GetString();
    }

    public static List<string> GetTraitIds(GameItem item)
    {
        var result = new List<string>();
        var species = HusbandryHelper.GetSpecies(item) ?? AnimalSpecies.Rat;
        if (species.geneticTraitIDs != null)
            foreach (var id in species.geneticTraitIDs)
                if (id != null) result.Add(id.ToString());
        return result;
    }

    public static bool HigherIsBetter(string trait)
    {
        bool v;
        return !HusbandryHelper.StatHigherIsBetter.TryGetValue(trait, out v) || v;
    }
}
