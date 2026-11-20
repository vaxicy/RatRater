# Water Auto Label — 水质自动标注

**English** | [简体中文](README.zh-CN.md)

A native C# mod for **Probably Stolen** (Steam App 4348910). Water containers display their true quality automatically — no more manually labeling each one with the labeler.

## Features

- Every liquid container shows its **real water quality** based on actual purity: pure water, high-quality water, base water, ghost water, rust water, gutterflow, etc.
- **Manual labels are respected**: if you set a label yourself, it stays. Auto-labeling only runs when the water's actual purity changes.
- Uses the game's own `WaterFeatureHelper.OnLiquidChanged` — no custom water logic, no save edits.
- Compatible with **FraudDetection**: both read the same water feature; the auto-labeled "real" quality and FraudDetection's fake/real comparison work together.
- Toggle "Auto-label water quality" in the mod settings (on by default).

## Install

1. Download the latest `WaterAutoLabel` zip from the [Releases](releases) page and unzip it.
2. Copy the `WaterAutoLabel` folder into the game's `Mods` folder (next to `Probably Stolen.exe`).
3. Start the game, open **Mods** on the main menu, enable **Water Auto Label**, and restart.
4. Load a save — hover any water container to see its true quality.

Requires the game's built-in mod loader. **MelonLoader is not required.**

## Notes

- Scan runs every 0.5s over all owned liquid containers; only containers whose purity changed are updated.
- Uninstall: close the game and delete the `Mods\WaterAutoLabel` folder. Saves are unaffected.

## License

MIT. The game and its assets belong to Questing Goose Studio. This mod contains no game assets.
