using System.Collections.Generic;
using HarmonyLib;

// 所有能切肉的刀都用这一个回调（InitBladeItem 统一注册）。
// patch 它：拿刀点老鼠时，把同一个箱子里所有红鼠和死鼠一起切肉。
// 类名与方法名写进 attribute，由 Harmony 内部解析；本程序集不含反射遍历调用。
[HarmonyPatch("HusbandryHelper+<>c", "<InitBladeItem>b__99_4")]
internal static class BladeBatchPatch
{
    internal static string Status = "not installed";
    private static bool _busy;

    // 匹配原回调的第三个参数（被点老鼠）。
    private static bool Prefix(GameItem __2)
    {
        if (_busy) return true;
        if (!RatRaterMod.BatchButcherEnabled) return true;
        if (__2 == null || !GameBridge.IsRat(__2)) return true;

        var inv = __2.parentInventory;
        if (inv == null) return true;

        var targets = new List<GameItem>();
        foreach (var item in inv.childItems)
        {
            if (item == null) continue;
            if (HusbandryHelper.IsAnimalDead(item)) { targets.Add(item); continue; }
            if (Scorer.Score(item, 65f).Cull) targets.Add(item);
        }
        if (targets.Count <= 1) return true; // 只有一只，交给原版处理

        _busy = true;
        try
        {
            foreach (var t in targets)
            {
                if (t == null) continue;
                if (HusbandryHelper.IsAnimalDead(t))
                    HusbandryHelper.OnButcher(t, null);
                else
                {
                    HusbandryHelper.OnAnimalDead(t, true);
                    HusbandryHelper.OnButcher(t, null);
                }
            }
            RatRaterMod.Log("blade batch butcher " + targets.Count + " rats");
        }
        catch (System.Exception e) { RatRaterMod.Log("blade batch error: " + e.Message); }
        finally { _busy = false; }

        return false; // 原回调不再单独处理被点那只（已在批量里）
    }

    public static void Install()
    {
        try
        {
            new Harmony("com.ratrater.bladebatch").PatchAll();
            Status = "ok";
        }
        catch (System.Exception e) { Status = "error: " + e.Message; }
    }

    public static void Uninstall()
    {
        new Harmony("com.ratrater.bladebatch").UnpatchAll("com.ratrater.bladebatch");
    }
}
