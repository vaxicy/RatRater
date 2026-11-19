using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// K 键一键屠宰：只处理鼠标当前指向的箱子/背包里的红鼠和死鼠。
internal static class ButcherService
{
    internal const string SkipConfirmKey = "RatRater.SkipButcherConfirm";

    private static readonly List<RaycastResult> _hits = new List<RaycastResult>();

    // 返回鼠标当前指向的库存（向上找 GameItemElement），找不到返回 null。
    public static GameInventory CurrentInventory()
    {
        var es = EventSystem.current;
        if (es == null) return null;
        _hits.Clear();
        var data = new PointerEventData(es) { position = Input.mousePosition };
        es.RaycastAll(data, _hits);
        foreach (var h in _hits)
        {
            if (h.gameObject == null) continue;
            var el = h.gameObject.GetComponentInParent<GameItemElement>();
            if (el == null || el.IsDestroyed()) continue;
            var inv = el.parentInventory;
            if (inv != null) return inv;
        }
        return null;
    }

    // 当前库存里要处理的老鼠（红鼠 + 死鼠），怀孕跳过。
    public static List<GameItem> CollectTargets(GameInventory inv, out int totalRats)
    {
        var list = new List<GameItem>();
        totalRats = 0;
        if (inv == null) return list;
        foreach (var item in inv.childItems)
        {
            if (item == null || !GameBridge.IsRat(item)) continue;
            totalRats++;
            if (HusbandryHelper.IsAnimalDead(item)) { list.Add(item); continue; }
            if (Scorer.Score(item, 65f).Cull) list.Add(item);
        }
        return list;
    }

    public static bool HasBlade()
    {
        var inst = EmporiumEntry.Instance;
        if (inst == null) return false;
        foreach (var it in inst.GetAllOwnedItems(false, true))
            if (it != null && it.IsTag(HusbandryHelper.BLADE_ITEM_TAG)) return true;
        return false;
    }

    // 处理一只：死的直接切；活的先杀死再切。
    public static void ButcherOne(GameItem item)
    {
        if (item == null) return;
        if (!HusbandryHelper.IsAnimalDead(item))
            HusbandryHelper.SetHealthToZero(item);
        HusbandryHelper.OnButcher(item, null);
    }

    public static bool SkipConfirm
    {
        get { return PlayerPrefs.GetInt(SkipConfirmKey, 0) == 1; }
        set { PlayerPrefs.SetInt(SkipConfirmKey, value ? 1 : 0); PlayerPrefs.Save(); }
    }

    // 弹确认面板；skip 勾选后写入 PlayerPrefs，下次直接杀。
    public static void AskConfirm(GameInventory inv, List<GameItem> targets, int totalRats)
    {
        var b = CustomUIManager.Window("ratrater.butcher", "一键屠宰确认", "overlay");
        b.AddLabel("当前箱子有 " + totalRats + " 只老鼠，其中 " + targets.Count + " 只要处理（红色建议淘汰 + 死鼠）。");
        b.AddLabel("活鼠会先杀死再切成肉，肉留在原箱子。怀孕母鼠不会处理。");
        bool skip = SkipConfirm;
        b.AddToggle("不再提醒（下次按 K 直接处理）", skip, v => { skip = v; });
        b.AddButton("确认处理 " + targets.Count + " 只", () =>
        {
            if (skip) SkipConfirm = true;
            RunButcher(inv, targets);
        });
        b.AddButton("取消", () => { });
        b.Show();
    }

    // 分帧处理，避免一次切几十只卡顿。
    private static readonly Queue<GameItem> _queue = new Queue<GameItem>();
    private static int _butcherFrame;

    public static void RunButcher(GameInventory inv, List<GameItem> targets)
    {
        if (inv == null || targets == null || targets.Count == 0) return;
        _queue.Clear();
        foreach (var t in targets) if (t != null) _queue.Enqueue(t);
        _butcherFrame = Time.frameCount;
        RatRaterMod.Log("butcher start " + _queue.Count + " rats");
    }

    // 由 ticker 每帧调用，每帧最多处理 4 只。
    public static void Tick()
    {
        if (_queue.Count == 0) return;
        if (Time.frameCount <= _butcherFrame) return;
        _butcherFrame = Time.frameCount;
        int done = 0;
        while (_queue.Count > 0 && done < 4)
        {
            var item = _queue.Dequeue();
            if (item != null) ButcherOne(item);
            done++;
        }
        RatRaterMod.Log("butcher tick -" + done + " left=" + _queue.Count);
    }
}
