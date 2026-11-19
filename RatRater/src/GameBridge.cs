using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

// 通过反射访问游戏类型，避免编译期符号不确定导致 mod 加载失败。
internal static class GameBridge
{
    private static readonly BindingFlags Any =
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    private static Type _tGameItem, _tElement, _tStore, _tHusbandry, _tGenetic, _tSpecies, _tTag;
    private static MethodInfo _mGetAllOwned, _mIsTag, _mGetTag, _mGetFloat, _mIsDead, _mGetSpecies;
    private static object _storeInstance, _statHigher, _ratSpecies;
    private static FieldInfo _fGeneticTraitIds;
    private static object _geneticHelper;

    public static bool Ready { get; private set; }
    public static string Diagnostics()
    {
        return "store=" + (_storeInstance != null) +
               " getAll=" + (_mGetAllOwned != null) +
               " isTag=" + (_mIsTag != null) +
               " getTag=" + (_mGetTag != null) +
               " species=" + (_ratSpecies != null) +
               " traits=" + (_fGeneticTraitIds != null);
    }

    public static bool Init()
    {
        try
        {
            var asm = typeof(IMod).Assembly;
            _tGameItem = asm.GetType("GameItem", true);
            _tElement = asm.GetType("GameItemElement", true);
            _tStore = asm.GetType("PlayerStore", true);
            _tHusbandry = asm.GetType("HusbandryHelper", true);
            _tGenetic = asm.GetType("GeneticHelper", true);
            _tSpecies = asm.GetType("AnimalSpecies", true);
            _tTag = asm.GetType("TagState", true);

            _mGetAllOwned = FindMethod(_tStore, "GetAllOwnedItems");
            _mIsTag = FindMethod(_tGameItem, "IsTag");
            _mGetTag = FindMethod(_tGameItem, "GetTagReadonly");
            _mGetFloat = FindMethod(_tTag, "GetFloat");
            _mIsDead = FindMethod(_tHusbandry, "IsAnimalDead");
            _mGetSpecies = FindMethod(_tHusbandry, "GetSpecies");

            var pInst = _tStore.GetProperty("Instance", Any) ?? _tStore.GetProperty("instance", Any);
            var fInst = _tStore.GetField("Instance", Any) ?? _tStore.GetField("instance", Any);
            _storeInstance = pInst != null ? pInst.GetValue(null, null) : (fInst != null ? fInst.GetValue(null) : null);

            _statHigher = GetStaticMember(_tHusbandry, "StatHigherIsBetter");
            _fGeneticTraitIds = _tSpecies.GetField("geneticTraitIDs", Any);

            var fRat = _tSpecies.GetField("Rat", Any);
            _ratSpecies = fRat != null ? fRat.GetValue(null) : null;

            var fGeneticInstance = _tGenetic.GetField("Instance", Any) ?? _tGenetic.GetField("instance", Any);
            _geneticHelper = fGeneticInstance != null ? fGeneticInstance.GetValue(null) : Activator.CreateInstance(_tGenetic);

            Ready = _storeInstance != null && _mGetAllOwned != null && _mIsTag != null && _mGetTag != null;
            return Ready;
        }
        catch (Exception e)
        {
            RatRaterMod.Log("GameBridge.Init error: " + e.Message);
            return false;
        }
    }

    private static MethodInfo FindMethod(Type t, string name)
    {
        if (t == null) return null;
        foreach (var m in t.GetMethods(Any))
            if (m.Name == name) return m;
        return null;
    }

    private static object GetStaticMember(Type t, string name)
    {
        var p = t.GetProperty(name, Any);
        if (p != null) return p.GetValue(null, null);
        var f = t.GetField(name, Any);
        return f != null ? f.GetValue(null) : null;
    }

    private static object Invoke(MethodInfo m, object target, params object[] args)
    {
        if (m == null) return null;
        try { return m.Invoke(target, args); }
        catch (Exception e) { RatRaterMod.Log("invoke " + m.Name + " error: " + e.Message); return null; }
    }

    public static object GetAllElements()
    {
        if (_storeInstance == null || _mGetAllOwned == null) return null;
        // 现成高亮 mod 用 GetAllOwnedItems(false, true)。
        try
        {
            var ps = _mGetAllOwned.GetParameters();
            if (ps.Length >= 2) return _mGetAllOwned.Invoke(_storeInstance, new object[] { false, true });
            if (ps.Length == 1) return _mGetAllOwned.Invoke(_storeInstance, new object[] { true });
            return _mGetAllOwned.Invoke(_storeInstance, null);
        }
        catch (Exception e) { RatRaterMod.Log("GetAllOwnedItems error: " + e.Message); return null; }
    }

    public static object GetItem(object element)
    {
        if (element == null) return null;
        foreach (var n in new[] { "handler", "item", "Item" })
        {
            var f = _tElement.GetField(n, Any);
            if (f != null) return f.GetValue(element);
            var p = _tElement.GetProperty(n, Any);
            if (p != null) return p.GetValue(element, null);
        }
        return null;
    }

    public static Transform GetRectTransform(object element)
    {
        if (element == null) return null;
        var p = _tElement.GetProperty("rectTransform", Any);
        return p != null ? p.GetValue(element, null) as Transform : null;
    }

    public static bool IsRat(object item)
    {
        if (item == null) return false;
        if (_mGetSpecies != null)
        {
            var sp = Invoke(_mGetSpecies, null, item);
            if (sp != null && _ratSpecies != null && sp == _ratSpecies) return true;
        }
        return HasTag(item, "rat") || HasTag(item, "animal");
    }

    public static bool IsDead(object item)
    {
        if (item == null || _mIsDead == null) return false;
        var r = Invoke(_mIsDead, null, item);
        return r is bool b && b;
    }

    public static bool HasTag(object item, string tag)
    {
        if (item == null || _mIsTag == null) return false;
        var r = Invoke(_mIsTag, item, tag);
        return r is bool b && b;
    }

    public static float GetFloatTag(object item, string tag)
    {
        if (!HasTag(item, tag)) return 0f;
        var ts = Invoke(_mGetTag, item, tag);
        if (ts == null) return 0f;
        var v = Invoke(_mGetFloat, ts);
        return v is float f ? f : 0f;
    }

    // 返回该物种的基因 trait id 列表（字符串集合）。
    public static List<string> GetTraitIds(object item)
    {
        var result = new List<string>();
        object species = null;
        if (_mGetSpecies != null) species = Invoke(_mGetSpecies, null, item);
        if (species == null) species = _ratSpecies;
        if (species == null || _fGeneticTraitIds == null) return result;
        var ids = _fGeneticTraitIds.GetValue(species) as IEnumerable;
        if (ids == null) return result;
        foreach (var o in ids) if (o != null) result.Add(o.ToString());
        return result;
    }

    public static bool HigherIsBetter(string trait)
    {
        var dict = _statHigher as IDictionary;
        if (dict == null) return true;
        foreach (DictionaryEntry e in dict)
            if (e.Key != null && e.Key.ToString() == trait)
                return e.Value is bool b && b;
        return true;
    }

    public static string GetStringTag(object item, string tag)
    {
        if (!HasTag(item, tag)) return null;
        var ts = Invoke(_mGetTag, item, tag);
        if (ts == null) return null;
        var m = FindMethod(_tTag, "GetString");
        var v = Invoke(m, ts);
        return v as string;
    }
}
