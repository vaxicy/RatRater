using System;
using System.Reflection;
using HarmonyLib;

// 把死老鼠排到后面：patch 游戏 InventorySortHelper 的排序比较器。
// 游戏的原生整理和 Duoji 一键整理最终都调用这个比较器，所以两者都生效。
internal static class SortPatch
{
    private static Harmony _harmony;
    private static MethodInfo _target;
    private static MethodInfo _isDead;
    private static Type _tHusbandry;
    internal static bool Installed { get; private set; }
    internal static string Status = "not installed";

    public static void Install()
    {
        try
        {
            var asm = typeof(IMod).Assembly;
            _tHusbandry = asm.GetType("HusbandryHelper");
            if (_tHusbandry != null)
                foreach (var m in _tHusbandry.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                    if (m.Name == "IsAnimalDead") { _isDead = m; break; }

            _target = FindSortComparator(asm);
            if (_target == null) { Status = "comparator not found"; return; }

            _harmony = new Harmony("com.ratrater.deadlast");
            var prefix = typeof(SortPatch).GetMethod("Prefix", BindingFlags.Static | BindingFlags.NonPublic);
            _harmony.Patch(_target, prefix: new HarmonyMethod(prefix));
            Installed = true;
            Status = "ok: " + _target.DeclaringType.FullName + "." + _target.Name;
        }
        catch (Exception e)
        {
            Status = "error: " + e.Message;
        }
    }

    private static MethodInfo FindSortComparator(Assembly asm)
    {
        var helper = asm.GetType("InventorySortHelper");
        if (helper == null) return null;
        foreach (var nt in helper.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
        {
            foreach (var m in nt.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            {
                if (m.Name == "<Sort>b__2") return m;
            }
        }
        return null;
    }

    // 两个 GameItem 比较。返回 true 表示我们给出了结果，走 __result。
    private static bool Prefix(ref int __result, object __0, object __1)
    {
        try
        {
            bool d1 = IsDead(__0);
            bool d2 = IsDead(__1);
            if (d1 == d2) return true; // 都是或都不是死老鼠：交给原比较器
            __result = d1 ? 1 : -1;   // 第一个死 → 排后面(正)；第二个死 → 排前面(负)
            return false;
        }
        catch
        {
            return true;
        }
    }

    private static bool IsDead(object item)
    {
        if (item == null || _isDead == null) return false;
        try { return _isDead.Invoke(null, new object[] { item }) is bool b && b; }
        catch { return false; }
    }

    public static void Uninstall()
    {
        if (_harmony != null) { _harmony.UnpatchAll("com.ratrater.deadlast"); _harmony = null; }
        Installed = false;
    }
}
