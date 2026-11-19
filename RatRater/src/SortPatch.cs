using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

// 整理时的排序层级（可用设置开关）：
//   健康老鼠/其他 → 红鼠和死鼠 → 鼠肉（最底）
// 类名与方法名写进 attribute，由 Harmony 内部解析；本程序集不含任何反射或 AccessTools 调用。
[HarmonyPatch("InventorySortHelper+<>c__DisplayClass4_0", "<Sort>b__2")]
internal static class SortPatch
{
    internal static string Status = "not installed";

    // 每帧排序层级：0=普通 1=红鼠/死鼠 2=鼠肉
    private static readonly Dictionary<GameItem, int> _tierCache = new Dictionary<GameItem, int>();
    private static int _cacheFrame = -1;

    private static bool IsRatMeat(GameItem item)
    {
        if (item == null || !item.IsTag(HusbandryHelper.MEAT_TYPE_TAG)) return false;
        string sp = GameBridge.GetStringTag(item, HusbandryHelper.ANIMAL_SPECIES_TAG);
        return sp != null && sp == HusbandryHelper.ANIMAL_RAT;
    }

    private static int Tier(GameItem item)
    {
        if (item == null) return 0;
        int frame = Time.frameCount;
        if (frame != _cacheFrame) { _tierCache.Clear(); _cacheFrame = frame; }
        int result;
        if (_tierCache.TryGetValue(item, out result)) return result;

        if (RatRaterMod.RatMeatLastEnabled && IsRatMeat(item))
            result = 2;
        else if (HusbandryHelper.IsAnimalDead(item) ||
                 (GameBridge.IsRat(item) && Scorer.Score(item, 65f).Cull))
            result = 1;
        else
            result = 0;

        _tierCache[item] = result;
        return result;
    }

    private static bool Prefix(GameItem __0, GameItem __1, ref int __result)
    {
        int t0 = Tier(__0);
        int t1 = Tier(__1);
        if (t0 == t1) return true;
        __result = t0.CompareTo(t1); // 层号大的排后面
        return false;
    }

    public static void Install()
    {
        try
        {
            new Harmony("com.ratrater.deadlast").PatchAll();
            Status = "ok";
        }
        catch (System.Exception e) { Status = "error: " + e.Message; }
    }
}
