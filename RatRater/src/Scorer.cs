using System.Collections.Generic;

internal sealed class RatScore
{
    public GameItem Item;
    public GameItemElement Element;
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
    // 六项基因的基准值（游戏平均值），按各自量纲归一化，不再统一除以 1.5。
    private static readonly string[] TraitTags =
    {
        HusbandryHelper.ANIMAL_GROWTH_RATE_TAG,
        HusbandryHelper.ANIMAL_LITTER_SIZE_TAG,
        HusbandryHelper.ANIMAL_IMMUNITY_TAG,
        HusbandryHelper.ANIMAL_MAX_HEALTH_TAG,
        HusbandryHelper.ANIMAL_LONGEVITY_TAG,
        HusbandryHelper.ANIMAL_HUNGER_RATE_TAG,
    };
    private static readonly string[] TraitLabels =
    {
        "生长速度", "窝仔数", "免疫力", "最大健康", "寿命", "饥饿速度"
    };
    private static readonly float[] TraitBaseline = { 1f, 3f, 30f, 100f, 1f, 65f };
    private static readonly bool[] LowerBetter = { false, false, false, false, false, true };

    public static string GetTagName(string field) { return null; }

    // tag 存的是 0~1 比例；个别版本可能存 0~100，统一成 0~1。
    private static float NormalizeRatio(float v)
    {
        if (v > 1f) return v / 100f;
        return v;
    }

    private static bool HasPositiveFlag(GameItem item, string tag)
    {
        return item.IsTag(tag) && GameBridge.GetFloatTag(item, tag) > 0f;
    }

    public static RatScore Score(GameItem item, float avgHunger)
    {
        var s = new RatScore { Item = item };

        // 六项基因评分：v / 基准值，越接近或超过基准越好；饥饿速度越低越好。
        float sum = 0f;
        int n = 0;
        for (int i = 0; i < TraitTags.Length; i++)
        {
            float v = GameBridge.GetFloatTag(item, TraitTags[i]);
            s.Genes.Add(new KeyValuePair<string, float>(TraitLabels[i], v));
            float score = v / TraitBaseline[i];
            if (score < 0f) score = 0f;
            if (score > 1.5f) score = 1.5f;
            if (LowerBetter[i]) score = 2f - score;
            if (score < 0f) score = 0f;
            if (score > 1f) score = 1f;
            sum += score;
            n++;
        }
        s.Total = n > 0 ? sum / n : 0f;

        string sex = GameBridge.GetStringTag(item, HusbandryHelper.ANIMAL_SEXE_TAG);
        s.Female = sex != null && (sex == "female" || sex == "1" || sex.ToLower().Contains("fem"));

        s.Pregnant = HasPositiveFlag(item, HusbandryHelper.ANIMAL_PREGNANT_TAG);
        s.Age = GameBridge.GetFloatTag(item, HusbandryHelper.ANIMAL_AGE_TAG);
        float longevity = GameBridge.GetFloatTag(item, HusbandryHelper.ANIMAL_LONGEVITY_TAG);
        if (longevity <= 0f) longevity = 1f;
        float maxAge = 14f * longevity;

        // 负面状态（直接读游戏自己的 tag）。
        bool starving = HasPositiveFlag(item, HusbandryHelper.ANIMAL_IS_STARVING_TAG);
        bool diseased = HasPositiveFlag(item, HusbandryHelper.ANIMAL_DISEASED_TAG);
        bool prion = HasPositiveFlag(item, HusbandryHelper.ANIMAL_PRION_TAG);
        bool scratch = HasPositiveFlag(item, HusbandryHelper.ANIMAL_SCRACH_MARK_TAG);
        float overcrowd = GameBridge.GetFloatTag(item, HusbandryHelper.ANIMAL_OVERCROWDING_TAG);
        float immunity = GameBridge.GetFloatTag(item, HusbandryHelper.ANIMAL_IMMUNITY_TAG);
        bool lowImmunity = immunity > 0f && immunity < 25f;
        bool old = s.Age >= maxAge;

        // 繁殖力：当前 <30% 或 基础 <50% 判红。tag 为 0~1 比例。
        float curFert = NormalizeRatio(GameBridge.GetFloatTag(item, HusbandryHelper.ANIMAL_CURRENT_FERTILITY_TAG));
        float baseFert = NormalizeRatio(GameBridge.GetFloatTag(item, HusbandryHelper.ANIMAL_FERTILITY_TAG));
        bool lowCurFert = curFert > 0f && curFert < 0.30f;
        bool lowBaseFert = baseFert > 0f && baseFert < 0.50f;

        var reasons = new List<string>();
        if (starving) reasons.Add("挨饿");
        if (diseased) reasons.Add("患病");
        if (prion) reasons.Add("朊病毒");
        if (scratch) reasons.Add("抓痕");
        if (overcrowd >= 2f) reasons.Add("过度拥挤");
        if (lowImmunity) reasons.Add("免疫力低下");
        if (old) reasons.Add("老年");
        if (lowCurFert) reasons.Add("当前繁殖力低");
        if (lowBaseFert) reasons.Add("基础繁殖力低");

        // 怀孕母鼠优先保护，不判淘汰。
        s.Cull = !s.Pregnant && reasons.Count > 0;
        if (s.Cull) s.Reason = string.Join(" / ", reasons.ToArray());

        return s;
    }
}
