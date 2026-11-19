using HarmonyLib;

// 把死老鼠排到后面：patch 游戏 InventorySortHelper 的排序比较器。
// 类名与方法名写进 attribute，由 Harmony 内部解析；本程序集不含任何反射或 AccessTools 调用。
[HarmonyPatch("InventorySortHelper+<>c__DisplayClass4_0", "<Sort>b__2")]
internal static class SortPatch
{
    internal static string Status = "not installed";

    private static bool Prefix(GameItem __0, GameItem __1, ref int __result)
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
            new Harmony("com.ratrater.deadlast").PatchAll();
            Status = "ok";
        }
        catch (System.Exception e) { Status = "error: " + e.Message; }
    }
}
