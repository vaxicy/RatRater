## 修复

- 修复 Ctrl+K 完全无反应的问题：日志显示 `GameBridge.GetAllElements` 在 `EmporiumEntry.Instance` 为空时每帧抛 NullReferenceException，中断了 Update 里的按键检测
- 给库存读取加空值保护，异常不再刷屏，按键正常触发

## 说明

沿用 1.5.0 的 Ctrl+K 一键屠宰。下载 RatRater-1.5.1.zip，把 RatRater 文件夹复制到游戏的 Mods 目录，启用并重启。
