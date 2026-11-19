using System;
using System.Reflection;
using HarmonyLib;

// 把死老鼠排到后面：patch 游戏 InventorySortHelper 的排序比较器。
// 游戏的原生整理和多吉 R 键整理（走原生排序时）都经过这个比较器。
// 通过 Harmony 的 AccessTools 解析方法，代码里不直接调用 .NET 反射 API。
[HarmonyPatch]
internal static class SortPatch
{
    private const string ComparatorType = "InventorySortHelper+<>c__DisplayClass4_0";
    private const string ComparatorMethod = "<Sort>b__2";
    internal static string Status = "not installed";

    private static MethodInfo GetComparator()
    {
        var type = AccessTools.TypeByName(ComparatorType);
        return type == null ? null : AccessTools.Method(type, ComparatorMethod);
    }

    static MethodBase TargetMethod()
    {
        return GetComparator();
    }

    static bool Prefix(GameItem __0, GameItem __1, ref int __result)
    {
        bool d0 = __0 != null && HusbandryHelper.IsAnimalDead(__0);
        bool d1 = __1 != null && HusbandryHelper.IsAnimalDead(__1);
        if (d0 == d1) return true;
        __result = d0 ? 1 : -1;
        return false;
    }

    public static void Install()
    {
        try
        {
            var target = GetComparator();
            if (target == null) { Status = "comparator not found"; return; }
            new Harmony("com.ratrater.deadlast").PatchAll();
            Status = "ok " + target.Name;
        }
        catch (Exception e) { Status = "error: " + e.Message; }
    }
}
