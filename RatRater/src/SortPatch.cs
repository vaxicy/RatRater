using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

// 整理时把死老鼠和红色（建议淘汰）老鼠排到最后，方便集中拖动处理。
// 类名与方法名写进 attribute，由 Harmony 内部解析；本程序集不含任何反射或 AccessTools 调用。
[HarmonyPatch("InventorySortHelper+<>c__DisplayClass4_0", "<Sort>b__2")]
internal static class SortPatch
{
    internal static string Status = "not installed";

    // 排序比较器会被高频调用，按帧缓存判定结果，避免每次比较都读一堆 tag。
    private static readonly Dictionary<GameItem, bool> _sinkCache = new Dictionary<GameItem, bool>();
    private static int _cacheFrame = -1;

    private static bool ShouldSink(GameItem item)
    {
        if (item == null) return false;
        int frame = Time.frameCount;
        if (frame != _cacheFrame)
        {
            _sinkCache.Clear();
            _cacheFrame = frame;
        }
        bool result;
        if (_sinkCache.TryGetValue(item, out result)) return result;

        result = HusbandryHelper.IsAnimalDead(item) ||
                 (GameBridge.IsRat(item) && Scorer.Score(item, 65f).Cull);
        _sinkCache[item] = result;
        return result;
    }

    private static bool Prefix(GameItem __0, GameItem __1, ref int __result)
    {
        bool s0 = ShouldSink(__0);
        bool s1 = ShouldSink(__1);
        if (s0 == s1) return true;
        __result = s0 ? 1 : -1;
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
