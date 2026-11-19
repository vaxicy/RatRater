using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
        try
        {
            _log = (ModLog)Activator.CreateInstance(typeof(ModLog),
                manifest != null ? manifest.ID : "RatRater");
        }
        catch { _log = null; }
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
        SortPatch.Uninstall();
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
        if (!GameBridge.Init())
        {
            Log("GameBridge not ready, retry on tick");
        }
        Scorer.Init(typeof(IMod).Assembly.GetType("HusbandryHelper"));

        try
        {
            var hook = typeof(IMod).Assembly.GetType("ModHook");
            var ev = hook.GetEvent("OnGameLoadedNormal",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (ev != null)
            {
                var handler = Delegate.CreateDelegate(ev.EventHandlerType, null,
                    typeof(RatRaterMod).GetMethod("OnGameLoaded",
                        BindingFlags.NonPublic | BindingFlags.Static));
                ev.AddEventHandler(null, handler);
            }
        }
        catch (Exception e) { Log("hook error: " + e.Message); }

        SortPatch.Install();
        Log("sortPatch " + SortPatch.Status);

        try
        {
            _ticker = new GameObject("RatRaterTicker");
            _tickerComp = _ticker.AddComponent<RatRaterTicker>();
            UnityEngine.Object.DontDestroyOnLoad(_ticker);
        }
        catch (Exception e) { Log("ticker error: " + e.Message); }

        _installed = true;
    }

    private static void OnGameLoaded()
    {
        GameBridge.Init();
        Log("game loaded");
    }
}

public sealed class RatRaterTicker : MonoBehaviour
{
    private HighlightRenderer _renderer = new HighlightRenderer();
    private float _next;
    private bool _shown = true;
    private int _lastCount = -1;
    private bool _loggedApi;

    private void Update()
    {
        if (!RatRaterMod.Enabled) return;

        if (Input.GetKeyDown(KeyCode.H))
        {
            _shown = !_shown;
            if (!_shown) _renderer.Clear();
            RatRaterMod.Log("highlight " + (_shown ? "on" : "off"));
        }
        if (Input.GetKeyDown(KeyCode.F11))
        {
            Export();
        }
        if (Input.GetKeyDown(KeyCode.LeftBracket) || Input.GetKeyDown(KeyCode.RightBracket))
        {
            RatRaterMod.Threshold += Input.GetKeyDown(KeyCode.RightBracket) ? 0.05f : -0.05f;
            if (RatRaterMod.Threshold < 0.1f) RatRaterMod.Threshold = 0.1f;
            if (RatRaterMod.Threshold > 0.9f) RatRaterMod.Threshold = 0.9f;
            RatRaterMod.Log("threshold=" + RatRaterMod.Threshold.ToString("F2"));
        }

        if (!_shown) return;
        if (Time.unscaledTime < _next) return;
        _next = Time.unscaledTime + 0.4f;

        if (!GameBridge.Ready) GameBridge.Init();
        if (!_loggedApi)
        {
            _loggedApi = true;
            RatRaterMod.Log("api " + GameBridge.Diagnostics());
        }
        var elements = GameBridge.GetAllElements();
        if (elements == null) return;

        var scores = new List<RatScore>();
        float hungerSum = 0f; int hungerN = 0;
        foreach (var el in (IEnumerable)elements)
        {
            var item = GameBridge.GetItem(el);
            if (item == null || !GameBridge.IsRat(item) || GameBridge.IsDead(item)) continue;
            float h = GameBridge.GetFloatTag(item, Scorer.GetTagName("ANIMAL_HUNGER_RATE_TAG"));
            if (h > 0) { hungerSum += h; hungerN++; }
            var s = Scorer.Score(item, hungerN > 0 ? hungerSum / hungerN : 65f);
            s.Element = el;
            scores.Add(s);
        }

        _renderer.Apply(scores, RatRaterMod.ShowKeep);
        if (scores.Count != _lastCount)
        {
            _lastCount = scores.Count;
            RatRaterMod.Log("rats=" + scores.Count + ", cull=" + CountCull(scores));
        }
    }

    private static int CountCull(List<RatScore> scores)
    {
        int n = 0;
        foreach (var s in scores) if (s.Cull) n++;
        return n;
    }

    public void Clear() { _renderer.Clear(); }

    private void Export()
    {
        try
        {
            var scores = new List<RatScore>();
            var elements = GameBridge.GetAllElements();
            if (elements == null) return;
            foreach (var el in (IEnumerable)elements)
            {
                var item = GameBridge.GetItem(el);
                if (item == null || !GameBridge.IsRat(item)) continue;
                scores.Add(Scorer.Score(item, 65f));
            }
            string dir = Path.Combine(Application.persistentDataPath, "RatRater");
            System.IO.Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, "rats_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv");
            using (var w = new StreamWriter(path, false, System.Text.Encoding.UTF8))
            {
                w.WriteLine("cull,reason,pregnant,sex,age,score,genes");
                foreach (var s in scores)
                {
                    string genes = "";
                    foreach (var g in s.Genes) genes += g.Key + "=" + g.Value.ToString("F2") + " ";
                    w.WriteLine(string.Join(",",
                        s.Cull ? "1" : "0", s.Reason, s.Pregnant ? "1" : "0",
                        s.Female ? "F" : "M", s.Age.ToString("F1"),
                        s.Total.ToString("F3"), "\"" + genes.Trim() + "\""));
                }
            }
            RatRaterMod.Log("exported " + path);
        }
        catch (Exception e) { RatRaterMod.Log("export error: " + e.Message); }
    }
}
