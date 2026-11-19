# RatRater — 深空当铺老鼠淘汰高亮模组

《深空当铺：可能是偷的》（**Probably Stolen**，Steam App 4348910）的原生 C# 模组。打开鼠笼就能一眼看出哪些老鼠该淘汰，不用悬停；整理背包时死老鼠自动排到最后。

Enhance **Probably Stolen** (Deep Space Pawn Shop) with at-a-glance rat culling highlights and dead-rat-last sorting. Built with the game's native mod loader — no MelonLoader required.

## 功能

### 鼠笼高亮（常驻，不用悬停）

老鼠所在格子盖一层半透明底色，老鼠图案本身不变：

| 颜色 | 含义 |
|---|---|
| 🔴 红色 | 建议淘汰（基因偏低 / 年长 11 天以上） |
| 🔵 蓝色 | 怀孕母鼠，强制保护，不判淘汰 |
| 🟢 绿色 | 优质留种 |

判定基于老鼠的六项基因：生长速度、窝仔数、免疫力、最大健康值、寿命、饥饿速度（越低越好），再结合年龄与怀孕状态。

### 死老鼠置底

按 R 整理背包 / 仓库时，死老鼠自动排到最后一格，活老鼠照常在前。

兼容多吉 R 键整理：它走**游戏原生排序**时置底生效；若在 F4 面板选了「智能排序」等第三方方案，那套算法不经过游戏排序器，置底不生效——切回「原生 / Native」预设即可。

### 快捷键

| 按键 | 作用 |
|---|---|
| `H` | 开关全部高亮（默认开） |
| `[` / `]` | 调低 / 调高淘汰阈值（默认 0.50，越低越少老鼠被判淘汰） |
| `F11` | 导出鼠群清单 CSV 到存档目录的 `RatRater` 文件夹 |

只改显示与排序，不写存档、不改老鼠外观。

## 安装

本模组使用游戏**自带的原生模组加载器**，不需要 MelonLoader，也不用额外前置。

1. 从 [Releases](../../releases) 下载 `RatRater-1.1.0.zip`。
2. 退出游戏，解压，把 `RatRater` 文件夹整个复制到游戏的 `Mods` 目录，例如：
   ```
   D:\Probably Stolen\Mods\RatRater\manifest.xml
   D:\Probably Stolen\Mods\RatRater\RatRater.dll
   ```
3. 启动游戏，主菜单点「模组 / Mods」，找到 **老鼠淘汰高亮 / Rat Rater** 并启用。
4. 按提示重启游戏，读档进入店铺，打开老鼠笼即可看到高亮。

注意：`manifest.xml` 必须直接在 `Mods\RatRater\` 下，不能多套一层 `Mods\Mods\RatRater`。

卸载：关闭游戏，删掉 `Mods\RatRater` 文件夹即可，存档不受影响。

## 兼容与要求

- 游戏：Probably Stolen 正式版（Steam，.VERSION 100-Rev12 / 清单标注 0.46D）
- 加载方式：游戏原生模组加载器（带 `manifest.xml` 的 C# DLL）
- 已声明能力：`reflection-external`（读游戏类型/字段）、`files`（F11 导出 CSV）

## 排查

游戏模组菜单若提示「使用了清单文件中未声明的权限」，说明 manifest 缺少 `Capabilities` 声明——本项目已声明 `reflection-external` 和 `files`，请确认使用的是最新版。

高亮或置底没生效时，在存档目录的 `Player.log` 搜 `[RatRater]`：

- `api store=True getAll=True isTag=True getTag=True traits=True` — 接口全部命中；有 `False` 则是游戏版本接口名不同
- `sortPatch ok: ...<Sort>b__2` — 死老鼠置底 patch 成功；`comparator not found` 则是排序器嵌套类名变了

把对应日志行发到 Issues 即可，反射封装让修复通常只需改一处。

## 开发

源码在 `src/`，反编译/分析脚本在 `tools/`，编译用 Roslyn（`csc.dll`），引用游戏 `Probably Stolen_Data\Managed` 里的 DLL，不依赖本机安装 .NET SDK。

```
tools/scan_types.py    # 扫描程序集类型/字段/方法
tools/dump_il.py       # 反编译方法 IL
tools/dump_refs.py     # dump 类型/成员引用与字符串
tools/build.ps1        # 用 Roslyn 编译 src/*.cs 到 build/RatRater.dll
```

- `src/GameBridge.cs`：反射封装游戏 API（老鼠、格子、基因）
- `src/Scorer.cs`：六项基因评分与淘汰判定
- `src/HighlightRenderer.cs`：高亮渲染
- `src/SortPatch.cs`：Harmony patch 游戏排序器，死老鼠置底
- `src/RatRaterMod.cs`：模组入口、ticker、H/F11 快捷键、CSV 导出

## English

**RatRater** is a native C# mod for **Probably Stolen** (Steam App 4348910, the "Deep Space Pawn Shop" game). It highlights rats worth culling directly in the cage grid — no hovering needed — and sorts dead rats to the end when you tidy an inventory. Red = cull, blue = pregnant (protected), green = keep. Press `H` to toggle highlights, `[` / `]` to adjust the cull threshold, `F11` to export a colony CSV. Sorting patch works with the popular Duoji R-key sort when it uses the game's native sorting path. Uses the game's built-in mod loader; **MelonLoader is not required**.

Install: unzip `RatRater-1.1.0.zip`, copy the `RatRater` folder into the game's `Mods` directory, enable it in the in-game Mods menu, then restart.

## 许可

MIT。游戏本体、名称与素材版权归 Questing Goose Studio 所有，本项目不含游戏资源。
