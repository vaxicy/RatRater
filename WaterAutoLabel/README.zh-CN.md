# Water Auto Label — 水质自动标注

[English](README.md) | **简体中文**

《深空当铺：可能是偷的》（**Probably Stolen**，Steam App 4348910）的原生 C# 模组。水容器自动显示真实水质，不用再拿标签机一只只贴。

## 功能

- 所有水容器按**真实纯度**显示对应水质：纯净水、优质水、基础水、鬼水、铁锈水、沟渠水等。
- **手动标签保留**：你自己贴的标签不会被覆盖；只有水质（纯度）实际变化时才自动更新。
- 直接调用游戏自带的 `WaterFeatureHelper.OnLiquidChanged`，不自己算水质、不写存档。
- 兼容 **FraudDetection（显示真实品质）**：两者读同一套水质 feature，自动标注的"真实水质"跟 FraudDetection 的真假对比互不干扰。
- 模组设置里有「自动标注水质」开关，默认开。

## 安装

1. 到 [Releases](https://github.com/vaxicy/RatRater/releases) 页面下载最新版本的 WaterAutoLabel zip 并解压。
2. 把 `WaterAutoLabel` 文件夹复制到游戏的 `Mods` 目录（跟 `Probably Stolen.exe` 同级）。
3. 启动游戏，主菜单点「模组 / Mods」，启用「水质自动标注」，按提示重启。
4. 读档后悬停任意水容器，即可看到真实水质。

使用游戏自带原生模组加载器，**不需要 MelonLoader**。

## 说明

- 每 0.5 秒扫描一次所有水容器，只对纯度变化的容器更新。
- 卸载：关掉游戏，删掉 `Mods\WaterAutoLabel` 文件夹即可，存档不受影响。

## 许可

MIT。游戏本体与素材归 Questing Goose Studio 所有，本模组不含游戏资源。
