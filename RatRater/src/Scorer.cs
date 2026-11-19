using System;
using System.Collections.Generic;
using System.Reflection;

internal sealed class RatScore
{
    public object Item;
    public object Element;
    public float Total;
    public bool Cull;
    public bool Pregnant;
    public bool Female;
    public float Age;
    public string Reason;
    public List<KeyValuePair<string, float>> Genes = new List<KeyValuePair<string, float>>();
}

internal static class Scorer
{
    // HusbandryHelper 里的静态字符串常量（tag 名），按字段名反射取值。
    private static readonly string[] TraitFields =
    {
        "ANIMAL_GROWTH_RATE_TAG",
        "ANIMAL_LITTER_SIZE_TAG",
        "ANIMAL_IMMUNITY_TAG",
        "ANIMAL_MAX_HEALTH_TAG",
        "ANIMAL_LONGEVITY_TAG",
        "ANIMAL_HUNGER_RATE_TAG",
    };

    private static readonly string[] TraitLabels =
    {
        "生长速度", "窝仔数", "免疫力", "最大健康", "寿命", "饥饿速度"
    };

    // 越小越好的基因。
    private static readonly bool[] LowerBetter = { false, false, false, false, false, true };

    private static Dictionary<string, string> _tagCache;
    private static Type _tHusbandry;

    private static string TagName(string field)
    {
        if (_tagCache == null) return null;
        string v;
        return _tagCache.TryGetValue(field, out v) ? v : null;
    }

    public static void Init(Type husbandry)
    {
        _tHusbandry = husbandry;
        _tagCache = new Dictionary<string, string>();
        if (husbandry == null) return;
        foreach (var f in husbandry.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
        {
            if (f.FieldType != typeof(string)) continue;
            try
            {
                var v = f.GetValue(null) as string;
                if (!string.IsNullOrEmpty(v)) _tagCache[f.Name] = v;
            }
            catch { }
        }
    }

    public static string GetTagName(string field) { return TagName(field); }

    public static RatScore Score(object item, float avgHunger)
    {
        var s = new RatScore { Item = item };
        float sum = 0f;
        int n = 0;

        for (int i = 0; i < TraitFields.Length; i++)
        {
            string tag = TagName(TraitFields[i]);
            if (string.IsNullOrEmpty(tag)) continue;
            float v = GameBridge.GetFloatTag(item, tag);
            s.Genes.Add(new KeyValuePair<string, float>(TraitLabels[i], v));

            // 饥饿速度单独用绝对阈值（越低越好）；其余按比例分。
            float score;
            if (i == 5)
                score = v <= avgHunger * 0.75f ? 1f : (v <= avgHunger ? 0.7f : (v <= avgHunger * 1.3f ? 0.4f : 0.1f));
            else
                score = v / 1.5f;
                if (score < 0f) score = 0f;
                if (score > 1f) score = 1f;

            // 高亮判定用统一的 0~1 分，越大越好。
            if (LowerBetter[i]) score = 1f - score;
            sum += score;
            n++;
        }

        s.Total = n > 0 ? sum / n : 0f;

        string sexTag = TagName("ANIMAL_SEXE_TAG");
        string sex = GameBridge.GetStringTag(item, sexTag);
        s.Female = sex != null && (sex == "female" || sex == "1" || sex.ToLower().Contains("fem"));

        string pregTag = TagName("ANIMAL_PREGNANT_TAG");
        s.Pregnant = GameBridge.GetFloatTag(item, pregTag) > 0f ||
                     GameBridge.HasTag(item, pregTag);

        string ageTag = TagName("ANIMAL_AGE_TAG");
        s.Age = GameBridge.GetFloatTag(item, ageTag);

        // 淘汰判定：综合分低 + 不是怀孕母鼠 + 年长或饥饿差。
        s.Cull = !s.Pregnant && (s.Total < RatRaterMod.Threshold || (s.Age >= 11f && s.Total < 0.72f));

        if (s.Cull)
            s.Reason = s.Age >= 11f ? "年长 / 基因偏低" : "基因偏低";

        return s;
    }
}
