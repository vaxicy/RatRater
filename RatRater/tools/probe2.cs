using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

internal static class Probe2
{
    static void Test()
    {
        var es = EventSystem.current;
        var data = new PointerEventData(es) { position = Input.mousePosition };
        var hits = new List<RaycastResult>();
        es.RaycastAll(data, hits);
        foreach (var h in hits)
        {
            var el = h.gameObject.GetComponentInParent<GameItemElement>();
            if (el != null)
            {
                GameInventory inv = el.parentInventory;
                Console.WriteLine(inv);
            }
        }
        // 刀：遍历已拥有物品
        foreach (var it in EmporiumEntry.Instance.GetAllOwnedItems(false, true))
            if (it.IsTag(HusbandryHelper.BLADE_ITEM_TAG)) Console.WriteLine("blade " + it);

        // 屠宰 API
        HusbandryHelper.SetHealthToZero(null);
        HusbandryHelper.OnButcher(null, null);
        HusbandryHelper.IsAnimalDead(null);
        // 确认框
        GenericConfirmationUIManager.Instance.OpenPanel("body", "ok", "cancel", null, null);
        // 存储
        PlayerPrefs.SetInt("x", 1);
        PlayerPrefs.GetInt("x", 0);
    }
}
