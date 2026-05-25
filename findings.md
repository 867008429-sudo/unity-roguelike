# 研究发现

更新时间：2026-05-25

## 项目当前资料

- `Assets/Docs/Current_Work_Status_And_Optimization.md` 存在，内容是当前工作状态与后续优化清单。
- 当前未在工作区成功读取到 `Assets/Docs/2D_Roguelike_Game_Design_Document.md`，需要后续确认是否未保存、被移动或未同步。
- 根目录此前没有 `task_plan.md`、`findings.md`、`progress.md`。

## 已完成系统摘要

- 游戏已恢复到可正常进入和操作的状态。
- 已修复 `Time.timeScale` 暂停恢复相关问题。
- 玩家 HUD 已升级为 `ModernHudRoot`。
- HP、XP、GOLD 等核心 HUD 文本已升级为 TMP。
- 金币显示已有滚动数字、描边、投影和缩放反馈。
- 玩家 HP 已有缓冲血条。
- 暂停面板已有弹出和淡入淡出。
- 木桶等可交互物已有 World Space 提示。
- 敌人受击、死亡、玩家受伤已有初步反馈。

## 当前重点风险

- 用户反馈上一轮 UI/功能出现“删减感”，下一阶段必须优先保留现有功能。
- UI 统一时不能直接隐藏旧 UI。
- 交互系统统一时不能改变原有输入习惯。
- Unity MCP / Play Mode 刷新曾出现 reconnect 或状态不稳定，后续验证可优先由用户手动触发 Unity 编译与 Play Mode。
- 当前 MedievalSharp 字体适合英文风格，但不支持中文，需要 fallback 字体方案。

## 下一阶段设计判断

- 第一目标不是继续加新系统，而是建立稳定基线。
- UI 优化应采用增量增强策略。
- 交互系统可以继续推进，但要保持木桶和宝箱的原输入语义。
- 战斗反馈、对象池和 Debug 面板是第二梯队。

## 关键文件

- `Assets/_Scripts/UIManager.cs`
- `Assets/_Scripts/UI/UITheme.cs`
- `Assets/_Scripts/UI/BufferedHealthBar.cs`
- `Assets/_Scripts/UI/InteractableHint.cs`
- `Assets/_Scripts/Interaction/IInteractable.cs`
- `Assets/_Scripts/Interaction/PlayerInteractionDetector.cs`
- `Assets/_Scripts/Interaction/DestructiblePropInteractable.cs`
- `Assets/_Scripts/Interaction/LootChestInteractable.cs`
- `Assets/_Scripts/PlayerController.cs`
- `Assets/_Scripts/PlayerStats.cs`
- `Assets/_Scripts/WaveManager.cs`
- `Assets/_Scripts/GameConfig.cs`

## 待确认事项

- 当前 Unity Console 是否存在编译错误。
- 当前 Play Mode 中 `ModernHudRoot` 是否正常生成。
- 当前死亡面板是否仍显示原本 UI。
- 当前升级/遗物面板是否保留完整选项信息。
- 是否需要重新生成或恢复正式策划案文档。
- 新增 `StartMenuManager` 后，正式场景启动时是否能稳定暂停后台波次。
- 开始界面中文按钮使用 legacy `Text` 和 `UITheme.GameFont`，避免 TMP 英文字体缺中文。
- 开始界面已接入 Unity 版 Button variant，但尚未经过 Unity Console 编译与 Play Mode 实测。

## shadcn/ui 学习发现

- shadcn/ui 源码已下载到 `ReferenceRepos/shadcn-ui`，未放入 `Assets`，避免 Unity 导入前端源码。
- shadcn/ui 的核心价值是组件拆分、主题 token、variant、状态管理和信息层级，不是直接照搬 React/Tailwind。
- 可借鉴到 Unity 的重点包括 Button variant、Card 拆分、Dialog 结构、Tabs 设置页、Tooltip、Field/Switch choice card。
- 已创建学习笔记：`Assets/Docs/Shadcn_UI_Study_Notes.md`。
- 第一轮可复用结论：先把 UI 差异沉到 `UITheme`，具体界面只选择 style，不直接散写颜色。

## 角色动画反馈第一轮发现

- 当前角色资源不依赖完整 Sprite Sheet/Animator Controller，适合先用程序动画统一状态接口。
- 玩家已有攻击位移/挥砍视觉与 Dash Trail，本轮保留这些逻辑，只新增状态层反馈，避免大改战斗系统。
- 怪物已有攻击预警和死亡碎裂，第一轮重点补 Idle/Move/Attack/Hurt 的运动反馈；死亡仍保留碎裂效果。
- 玩家死亡已由 `PlayerStats.Die()` 禁用 `PlayerController` 锁定输入，本轮只叠加 Death 程序动画，不改死亡 UI 流程。

## 战斗爽感第一轮发现

- `PlayerController` 已有三段 combo 的伤害、范围、角度和击退差异，适合优先把差异外显到斩击尺寸、前摇/后摇和命中反馈，而不是重写攻击系统。
- `VisualEffectsManager` 已有 slash / particle 池，可以继续复用池对象做玩家斩击和地面脉冲，避免每次攻击新建大量临时物。
- `PlayerStats.TakeDamage()` 已经在闪避时直接免伤，是接入“完美闪避”的低风险入口；只需在免伤分支尝试触发奖励，不改变原受伤流程。
- 中文世界飘字继续复用 `TextMesh + UITheme.GameFont`，比 TMP 更稳定，适合“完美闪避”等短提示。

## 场景优化第一轮发现

- 当前 `DungeonAdventurer` 已有 KayKit 地砖、墙、道具和火把基础，但主光偏平，火把在画面里更像小亮点而不是空间光源。
- KayKit Dungeon 模型资源很丰富，适合先在场景边缘做道具簇和视觉兴趣点，不应堆在中心战斗区域。
- Game View 截图检查很有必要：第一版中边缘大件和假阴影垫过重，第二版已删除假阴影垫并缩小大件。
- 后续第二轮可以继续补：动态火焰粒子、墙面旗帜/窗洞的节奏、摄像机视野内前景遮挡层级，以及更系统的关卡路径引导。
