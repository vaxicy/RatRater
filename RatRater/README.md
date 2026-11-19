# RatRater — Culling Highlighter Mod for Probably Stolen

**English** | [简体中文](README.zh-CN.md)

A native C# mod for **Probably Stolen** (Steam App 4348910, the "Deep Space Pawn Shop" game). Open a rat cage and instantly see which rats are worth culling — no hovering needed. When you tidy an inventory, rats marked for culling and dead rats sort to the end so you can drag them away in one go. Uses the game's built-in mod loader; **MelonLoader is not required**.

## Features

### Cage Highlights (always on, no hovering)

Each rat's grid cell gets a translucent color overlay. The rat sprite itself is untouched.

| Color | Meaning |
|---|---|
| 🔴 Red | Cull recommended — starving / diseased / prion / scratch marks / overcrowding ≥2 / low immunity (<25) / old age / low fertility |
| 🔵 Blue | Pregnant female — protected, never marked for culling |
| 🟢 Green | No negative status, and genetics above the game's baseline |

Red is driven by the game's own status flags, not a made-up formula. Fertility counts too: current fertility below 30% **or** base fertility below 50% turns a rat red. Old age means age ≥ 14 × the longevity gene. Genetics are scored against in-game baselines (growth 1, litter 3, immunity 30, max health 100, longevity 1, hunger 65 — lower hunger is better).

### Cull Rats Last

During a sort, rats marked red (recommended for culling) and dead rats move to the last slots. Healthy rats stay in front, so you can select and drag the cull group in one motion. This mod does not add its own sort hotkey — it works with the game's built-in sort and compatible sorting mods.

Works with the popular Duoji R-key sort: this applies when Duoji uses the **game's native sorting path**. If you pick a third-party mode such as "smart sort" in the F4 panel, that algorithm bypasses the game's comparator and the feature won't apply — switch back to the "Native" preset.

### Hotkeys

| Key | Action |
|---|---|
| `H` | Toggle all highlights (on by default) |
| `K` | One-click butcher (see below) |

### One-Click Butcher

Point at a rat cage, backpack or container and press `K`. A confirmation panel shows how many rats in that container will be processed. On confirm, red rats are killed and butchered, dead rats are butchered directly, and the meat is placed back into the same container. Pregnant rats (blue) are left alone. Requires you to own a blade item. Tick "don't ask again" to skip the confirmation in the future (stored in PlayerPrefs).

No save edits, no sprite changes, no sensitive capabilities — activation won't show a "potential danger" warning.

## Installation

The mod uses the game's **built-in native mod loader**. No MelonLoader, no prerequisites.

1. Download `RatRater-1.4.1.zip` from [Releases](../../releases).
2. Exit the game, unzip, and copy the whole `RatRater` folder into the game's `Mods` directory, e.g.:
   ```
   D:\Probably Stolen\Mods\RatRater\manifest.xml
   D:\Probably Stolen\Mods\RatRater\RatRater.dll
   ```
3. Start the game, open **Mods** on the main menu, find **老鼠淘汰高亮 / Rat Rater**, and enable it.
4. Restart when prompted, load a save, enter the shop, and open a rat cage to see the highlights.

The `manifest.xml` must sit directly in `Mods\RatRater\` — not one level deeper at `Mods\Mods\RatRater`.

To uninstall, close the game and delete the `Mods\RatRater` folder. Saves are unaffected.

## Compatibility & Requirements

- Game: Probably Stolen retail build (Steam, `.VERSION 100-Rev12`; manifest declares `0.46D`)
- Loader: the game's native mod loader (a C# DLL with `manifest.xml`)
- No declared capabilities: no runtime reflection, no arbitrary file access

## Troubleshooting

This mod uses no runtime reflection and reads/writes no arbitrary files, so `Capabilities` in the manifest is empty and no "potential danger" dialog should appear. If one does, you are on an old version — update to 1.4.1.

If highlights or cull-rats-last don't work, search `Player.log` in your save directory for `[RatRater]`:

- `sortPatch ok ...` — the sorting patch loaded; `comparator not found` means the game's comparator class name changed
- `rats=N, cull=M` — the mod is scanning rats; if this line never appears, the game UI wasn't detected
- `create mark error` — the highlight overlay couldn't be attached

Post the relevant log line to Issues. Fixes usually need changes in one place only.

## Development

Source lives in `src/`, reverse-engineering helpers in `tools/`. Builds use Roslyn (`csc.dll`) referencing the DLLs in `Probably Stolen_Data\Managed` — no .NET SDK install needed.

```
tools/scan_types.py    # list assembly types / fields / methods
tools/dump_il.py       # disassemble method IL
tools/dump_refs.py     # dump type / member references and strings
tools/build.ps1        # compile src/*.cs into build/RatRater.dll with Roslyn
```

- `src/GameBridge.cs` — direct references to game APIs (rats, elements, genetics)
- `src/Scorer.cs` — six-trait scoring and cull decision
- `src/HighlightRenderer.cs` — highlight overlay rendering
- `src/SortPatch.cs` — Harmony patch on the game's sort comparator to sink cull/dead rats
- `src/RatRaterMod.cs` — mod entry, ticker, H toggle

## License

MIT. The game itself, its name, and its assets belong to Questing Goose Studio. This project contains no game assets.
