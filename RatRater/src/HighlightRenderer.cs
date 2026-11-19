using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

internal sealed class HighlightRenderer
{
    private readonly Dictionary<object, GameObject> _marks = new Dictionary<object, GameObject>();
    private static readonly Color CullColor = new Color(1f, 0.15f, 0.15f, 0.42f);
    private static readonly Color KeepColor = new Color(0.15f, 0.85f, 0.3f, 0.30f);
    private static readonly Color PregColor = new Color(0.2f, 0.55f, 1f, 0.32f);

    public void Apply(List<RatScore> scores, bool showKeep)
    {
        var seen = new HashSet<object>();
        foreach (var s in scores)
        {
            if (s.Element == null) continue;
            seen.Add(s.Element);

            Color? c = null;
            if (s.Cull) c = CullColor;
            else if (s.Pregnant) c = PregColor;
            else if (showKeep && s.Total >= 0.8f) c = KeepColor;

            if (!c.HasValue)
            {
                Remove(s.Element);
                continue;
            }

            GameObject mark;
            if (!_marks.TryGetValue(s.Element, out mark) || mark == null)
            {
                mark = Create(s.Element);
                if (mark == null) continue;
                _marks[s.Element] = mark;
            }
            var img = mark.GetComponent<Image>();
            if (img != null) img.color = c.Value;
            mark.SetActive(true);
        }

        // 清理已消失元素的标记。
        var stale = new List<object>();
        foreach (var kv in _marks) if (!seen.Contains(kv.Key)) stale.Add(kv.Key);
        foreach (var k in stale) Remove(k);
    }

    private GameObject Create(object element)
    {
        try
        {
            var rt = GameBridge.GetRectTransform(element) as RectTransform;
            if (rt == null) return null;
            var go = new GameObject("RatRaterMark", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(rt, false);
            var irt = go.GetComponent<RectTransform>();
            irt.anchorMin = Vector2.zero;
            irt.anchorMax = Vector2.one;
            irt.offsetMin = Vector2.zero;
            irt.offsetMax = Vector2.zero;
            var img = go.GetComponent<Image>();
            img.raycastTarget = false;
            img.color = CullColor;
            go.transform.SetAsLastSibling();
            return go;
        }
        catch (Exception e)
        {
            RatRaterMod.Log("create mark error: " + e.Message);
            return null;
        }
    }

    private void Remove(object element)
    {
        GameObject go;
        if (_marks.TryGetValue(element, out go))
        {
            if (go != null) UnityEngine.Object.Destroy(go);
            _marks.Remove(element);
        }
    }

    public void Clear()
    {
        foreach (var kv in _marks) if (kv.Value != null) UnityEngine.Object.Destroy(kv.Value);
        _marks.Clear();
    }
}
