using System;
using UnityEngine;

internal static class Probe3
{
    static void Test()
    {
        var b = CustomUIManager.Window("ratrater", "Kill rats?", "title");
        b.AddLabel("This will kill N rats in this container.");
        bool skip = false;
        b.AddToggle("Don't ask again", false, v => { skip = v; });
        b.AddButton("Confirm", () => { PlayerPrefs.SetInt("k", skip ? 1 : 0); });
        b.AddButton("Cancel", () => { });
        b.Show();
    }
}
