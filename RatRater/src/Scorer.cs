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

    private static readonly bool[] LowerBetter = { false, false, false, false, false, true };

    public static string GetTagName(string field) { return null; }

    public static RatScore Score(GameItem item, float avgHunger)
    {
        var s = new RatScore { Item = item };
        float sum = 0f;
        int n = 0;

        for (int i = 0; i < TraitTags.Length; i++)
        {
            float v = GameBridge.GetFloatTag(item, TraitTags[i]);
            s.Genes.Add(new KeyValuePair<string, float>(TraitLabels[i], v));

            float score;
            if (i == 5)
                score = v <= avgHunger * 0.75f ? 1f : (v <= avgHunger ? 0.7f : (v <= avgHunger * 1.3f ? 0.4f : 0.1f));
            else
                score = v / 1.5f;
            if (score < 0f) score = 0f;
            if (score > 1f) score = 1f;

            if (LowerBetter[i]) score = 1f - score;
            sum += score;
            n++;
        }

        s.Total = n > 0 ? sum / n : 0f;

        string sex = GameBridge.GetStringTag(item, HusbandryHelper.ANIMAL_SEXE_TAG);
        s.Female = sex != null && (sex == "female" || sex == "1" || sex.ToLower().Contains("fem"));

        string pregTag = HusbandryHelper.ANIMAL_PREGNANT_TAG;
        s.Pregnant = item.IsTag(pregTag) && GameBridge.GetFloatTag(item, pregTag) > 0f;

        s.Age = GameBridge.GetFloatTag(item, HusbandryHelper.ANIMAL_AGE_TAG);

        s.Cull = !s.Pregnant && (s.Total < RatRaterMod.Threshold || (s.Age >= 11f && s.Total < 0.72f));
        s.Reason = s.Cull ? (s.Age >= 11f ? "年长 / 基因偏低" : "基因偏低") : null;

        return s;
    }
}
