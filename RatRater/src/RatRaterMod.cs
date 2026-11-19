using System.Collections.Generic;
using UnityEngine;

public sealed class RatRaterMod : IMod
{
    public static float Threshold = 0.5f;
    public static bool Enabled = true;
    public static bool ShowKeep = true;
    private static ModLog _log;
    private static GameObject _ticker;
    private static bool _installed;
    private static RatRaterTicker _tickerComp;

    public static void Log(string msg)
    {
        if (_log != null) _log.Log(msg);
        else Debug.Log("[RatRater] " + msg);
    }

    public void Init(ModManifest manifest)
    {
        _log = new ModLog(manifest);
        Log("init");
    }

    public void OnEnable()
    {
        Enabled = true;
        Install();
        Log("enabled");
    }

    public void OnDisable()
    {
        Enabled = false;
        if (_tickerComp != null) _tickerComp.Clear();
        if (_ticker != null) UnityEngine.Object.Destroy(_ticker);
        _ticker = null;
        _tickerComp = null;
        _installed = false;
        Log("disabled");
    }

    private void Install()
    {
        if (_installed) return;
        GameBridge.Init();
        ModHook.OnGameLoadedNormal += OnGameLoaded;

        SortPatch.Install();
        Log("sortPatch " + SortPatch.Status);

        _ticker = new GameObject("RatRaterTicker");
        _tickerComp = _ticker.AddComponent<RatRaterTicker>();
        UnityEngine.Object.DontDestroyOnLoad(_ticker);
        _installed = true;
    }

    private static void OnGameLoaded()
    {
        Log("game loaded");
    }
}

public sealed class RatRaterTicker : MonoBehaviour
{
    private HighlightRenderer _renderer = new HighlightRenderer();
    private float _next;
    private bool _shown = true;
    private int _lastCount = -1;

    private void Update()
    {
        if (!RatRaterMod.Enabled) return;

        if (Input.GetKeyDown(KeyCode.H))
        {
            _shown = !_shown;
            if (!_shown) _renderer.Clear();
            RatRaterMod.Log("highlight " + (_shown ? "on" : "off"));
        }
        ButcherService.Tick();

        bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        if (ctrl && Input.GetKeyDown(KeyCode.K))
        {
            TryButcher();
        }

        if (!_shown) return;
        if (Time.unscaledTime < _next) return;
        _next = Time.unscaledTime + 0.4f;

        var scores = new List<RatScore>();
        float hungerSum = 0f; int hungerN = 0;
        foreach (var item in GameBridge.GetAllElements())
        {
            if (item == null || !GameBridge.IsRat(item) || GameBridge.IsDead(item)) continue;
            float h = GameBridge.GetFloatTag(item, HusbandryHelper.ANIMAL_HUNGER_RATE_TAG);
            if (h > 0) { hungerSum += h; hungerN++; }
            var s = Scorer.Score(item, hungerN > 0 ? hungerSum / hungerN : 65f);
            s.Element = GameBridge.AsElement(item);
            if (s.Element != null) scores.Add(s);
        }

        _renderer.Apply(scores, RatRaterMod.ShowKeep);
        if (scores.Count != _lastCount)
        {
            _lastCount = scores.Count;
            int cull = 0;
            foreach (var s in scores) if (s.Cull) cull++;
            RatRaterMod.Log("rats=" + scores.Count + ", cull=" + cull);
        }
    }

    private static void TryButcher()
    {
        if (!ButcherService.HasBlade())
        {
            RatRaterMod.Log("butcher: no blade found");
            return;
        }
        var inv = ButcherService.CurrentInventory();
        if (inv == null)
        {
            RatRaterMod.Log("butcher: point at a container first");
            return;
        }
        int total;
        var targets = ButcherService.CollectTargets(inv, out total);
        if (targets.Count == 0)
        {
            RatRaterMod.Log("butcher: no cull/dead rats here");
            return;
        }
        if (ButcherService.SkipConfirm)
            ButcherService.RunButcher(inv, targets);
        else
            ButcherService.AskConfirm(inv, targets, total);
    }

    public void Clear() { _renderer.Clear(); }
}
