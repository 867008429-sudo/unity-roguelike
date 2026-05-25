# 下一阶段开发任务规划

计划创建时间：2026-05-25

## 目标声明

在不删减现有 UI 和玩法功能的前提下，把当前 2D Roguelike 项目推进到更稳定、更可扩展的下一阶段。重点顺序是：先稳住流程和 QA，再逐步统一 UI 与交互，随后增强战斗反馈、对象池和开发调试工具。

## 当前阶段

阶段 1：稳定性修复与回归 QA

状态：in_progress

状态说明：

- 已多次通过 UnityMCP 编译刷新，当前记录中无项目级 Console Error。
- 已完成若干短 Play Mode 烟测，确认正式场景可进入、开始菜单会暂停、退出后 `Time.timeScale = 1`。
- 仍缺完整人工回归：移动、攻击、闪避、交互、暂停恢复、升级、遗物、死亡 UI 的一轮连续验证。

## 阶段列表

### 阶段 0：规划与基线确认

状态：complete

目标：

- 汇总当前项目已完成内容和风险。
- 明确下一阶段任务边界。
- 建立 `task_plan.md`、`findings.md`、`progress.md` 三个规划文件。

交付物：

- 根目录 `task_plan.md`
- 根目录 `findings.md`
- 根目录 `progress.md`

验收标准：

- 三个规划文件存在。
- 计划中有明确阶段、优先级和验收标准。

### 阶段 1：稳定性修复与回归 QA

状态：in_progress

目标：

- 确认上一轮 UI / 交互改动没有删减原功能。
- 修复任何编译错误、启动错误、Play Mode 阻断问题。
- 建立最小回归清单。

任务：

- 检查 `UIManager.cs` 是否保留旧 UI 功能和死亡面板显示。
- 检查交互系统是否保持木桶 `E/J/鼠标左键`、宝箱 `E` 的原始输入语义。
- Play Mode 验证进入游戏、移动、攻击、闪避、暂停、恢复。
- 验证升级面板、遗物面板、死亡面板、金币滚动、血条缓冲。
- 检查 Console 是否有项目级 Error / Warning。

验收标准：

- 游戏可正常进入。
- 玩家可移动、攻击、闪避。
- `Time.timeScale` 初始为 `1`，暂停恢复后仍为 `1`。
- 木桶、宝箱交互符合原设定。
- HUD、升级、遗物、死亡 UI 都能显示。

### 阶段 2：UI 统一但不删减现有功能

状态：pending

目标：

- 保留现有 UI 信息和功能，只逐步提升视觉一致性。
- 避免再次出现“优化等于替换/删减”的问题。

任务：

- 为 UI 调整建立“只增强、不隐藏、不删除”的规则。
- 统一 `UITheme.cs` 中标题、正文、数值、按钮、面板颜色。
- 为 HUD 的 HP、XP、GOLD 增加图标，但保留原文本。
- 暂停、升级、遗物、死亡面板逐步 TMP 化，前提是先确认旧文本映射完整。
- 建立 UI 回归截图清单。

验收标准：

- 所有原有 UI 文本仍可见或有等价新版文本。
- 所有原有按钮/快捷键仍可用。
- 没有旧面板被无确认隐藏。

新增子任务：开始界面

状态：smoke_verified

内容：

- 新增运行时开始界面。
- 菜单包含新游戏、读入存档、设置、退出游戏。
- 进入场景后先暂停游戏，点击新游戏后恢复。
- 设置页先接入主音量滑条。
- 读入存档当前提供占位提示，等待存档系统接入。

验收标准：

- 进入正式场景后显示开始界面。
- 新游戏按钮可隐藏菜单并恢复 `Time.timeScale = 1`。
- 读入存档按钮在无存档时提示“暂无可用存档”。
- 设置按钮可进入设置页，返回按钮可回主菜单。
- 退出游戏按钮调用 `Application.Quit()`。

验证记录：

- Play Mode 烟测确认进入 `DungeonAdventurer` 后开始菜单下 `Time.timeScale = 0`。
- 退出 Play Mode 后确认 `Application.isPlaying = False`、`Time.timeScale = 1`。
- 后续仍建议做一次人工按钮逐项点击回归。

新增子任务：开始界面按钮 variant 与设置 Field 化

状态：compile_verified_pending_button_qa

内容：

- 扩展 `UITheme.cs` 的主题 token 和 `UIButtonStyle`。
- 为开始界面按钮接入 Primary、Secondary、Outline、Danger、Ghost 五类样式。
- 将设置页主音量整理为 Field 行结构，保留原 Slider 行为。

验收标准：

- 新游戏、读入存档、设置、退出游戏、返回按钮视觉层级清晰。
- 主音量 Slider 仍能调整 `AudioListener.volume`。
- 开始界面所有原按钮行为不变。
- Unity Console 无编译错误。

验证记录：

- `UITheme.cs` 与 `StartMenuManager.cs` 已通过静态结构检查。
- UnityMCP 编译刷新记录为无项目级 Error。
- 仍缺完整按钮行为与音量 Slider 人工回归。

### 阶段 3：交互系统完善

状态：pending

目标：

- 使用统一交互接口管理可交互物。
- 同屏多个交互物时只显示一个最合理提示。
- 不改变每种交互物原本的输入习惯。

任务：

- 完善 `IInteractable`。
- 完善 `PlayerInteractionDetector`。
- 木桶、宝箱、后续遗物/祭坛/传送门都走统一交互。
- 增加键鼠提示图标。
- 预留手柄输入提示接口。

验收标准：

- 多个交互物重叠时只显示一个提示。
- 木桶仍支持 `E/J/鼠标左键`。
- 宝箱只响应 `E`。
- 离开范围后提示稳定消失。

### 阶段 4：战斗反馈增强

状态：pending

目标：

- 提升命中、暴击、受击、死亡的反馈。
- 保证任何慢动作或 hit stop 都有自动恢复保护。

任务：

- 普通攻击、暴击、燃烧、震荡使用不同伤害数字颜色和节奏。
- 命中时增加轻微摄像机震动。
- 玩家受击联动屏幕红闪、闪白、击退、血条缓冲。
- 敌人死亡增加专属粒子、尸体淡出或碎裂残留。
- 为 hit stop 增加恢复保护。

验收标准：

- 命中反馈可明显感知。
- 暴击反馈和普通命中区分明确。
- 玩家受击不影响输入恢复。
- 不再出现慢动作无法恢复。

### 阶段 5：对象池与资源管理

状态：pending

目标：

- 降低高波次下频繁 `Instantiate / Destroy` 带来的性能风险。

任务：

- 新增统一 `PoolManager`。
- 池化伤害数字。
- 池化粒子特效。
- 池化金币/临时飘字/临时 UI。
- 增加自动回收时间。

验收标准：

- 常用特效和飘字不再每次新建销毁。
- 高波次战斗时 GC 压力下降。
- 池对象回收后状态正确重置。

### 阶段 6：QA 场景与 Debug 面板

状态：in_progress

目标：

- 提高后续开发验证效率。
- 避免每次改 UI 或战斗都手动跑完整流程。

任务：

- 新建 `QA_Sandbox` 场景。
- 放置玩家、敌人、木桶、宝箱、金币、升级触发器。
- 增加 Debug 面板。
- Debug 面板支持加金币、扣血、升级、生成敌人、暂停、死亡、清场。
- 增加启动自检脚本。

验收标准：

- 可以一键测试核心 UI 和战斗反馈。
- 可以快速复现暂停、升级、死亡、交互。
- QA 场景不影响正式场景。

## 优先级总览

| 优先级 | 内容 | 原因 |
|---|---|---|
| P0 | 稳定性修复与回归 QA | 防止继续在不稳定基线上叠功能 |
| P1 | UI 统一但不删减功能 | 用户已明确反馈不希望删减已有 UI |
| P2 | 交互系统完善 | 后续宝箱、祭坛、商店、传送门都要复用 |
| P3 | 战斗反馈增强 | 直接影响可玩性和爽感 |
| P4 | 对象池 | 高波次性能基础 |
| P5 | QA 场景和 Debug 面板 | 提升长期迭代效率 |

### 新增子任务：角色动画反馈第一轮

状态：compile_verified_pending_playmode_qa

内容：

- 新增统一程序动画控制器，先支持 Idle、Move、Attack、Dash、Hurt、Death。
- 新增玩家动画 Driver，接入移动、攻击、闪避、受伤、死亡。
- 新增怪物动画 Driver，接入巡逻/追击、攻击、受伤、死亡。
- 保留现有 VFX、攻击判定、Dash Trail、死亡碎裂和死亡 UI，不做系统性重构。

验收标准：

- 玩家移动/停止能在 Move/Idle 之间切换。
- 玩家攻击、闪避、受伤、死亡有可见程序动画反馈。
- 怪物巡逻/追击、攻击前摇、受伤、死亡有可见反馈。
- Unity Console 无编译错误。
- Play Mode 中原攻击、闪避、受伤、死亡逻辑不回退。

验证记录：

- 新增/修改脚本已通过本地静态结构检查。
- UnityMCP 编译刷新记录为 Console Error/Warning 0。
- 尚未完成移动、攻击、闪避、受伤、死亡的 Play Mode 可见反馈逐项验证。

### 新增子任务：战斗爽感第一轮

状态：smoke_verified_pending_gameplay_qa

内容：

- 三段普攻拥有不同斩击尺寸、颜色、前摇/后摇节奏和命中反馈强度。
- 暴击、燃烧、闪电流派命中时增加对应颜色的地面脉冲和爆发反馈，让流派更容易被玩家看出来。
- 闪避起步增加蓝色地面脉冲，成功在闪避窗口内规避伤害时触发“完美闪避”飘字、蓝色爆发，并返还一小段攻击/闪避冷却。
- 保留现有输入、伤害判定、升级/流派数值和 UI，不做系统重构。

验收标准：

- Unity Console 无编译错误。
- 普攻 1/2/3 段在视觉和节奏上能区分。
- 三条流派触发时的颜色和范围反馈更清楚。
- 闪避规避敌人攻击或酸液 tick 时能触发完美闪避反馈，且不影响原闪避无敌逻辑。

验证记录：

- 相关脚本已通过本地静态结构检查。
- UnityMCP 编译刷新成功，Console 无项目 Error。
- 短 Play Mode 烟测确认进入正式场景、开始菜单暂停、退出后 `Time.timeScale = 1`。
- 尚未完成三段普攻、流派触发、完美闪避的完整实机玩法 QA。

### 新增子任务：程序动画增强第二轮

状态：smoke_verified_pending_gameplay_qa

内容：

- 重做 `CharacterAnimationController` 的程序动画曲线，让 Idle、Move、Attack、Dash、Hurt、Death 都有更明显的节奏和重量。
- Idle 加入呼吸和轻微浮动，Move 加入身体倾斜、脚步节奏、启动/停止惯性和敌人差异化运动口味。
- 玩家攻击改为由动画层统一驱动，按 1/2/3 段区分轻击、横扫、重击，避免 `PlayerController` 与动画层同时抢同一个 `visualRoot`。
- 闪避加入起步压缩、中段拉伸和结束刹车，第三段攻击临时强化拖尾。
- 受击加入闪白、后仰、短暂失衡，死亡改为倒下/缩放/淡出式程序动画。
- 骷髅冲锋前接入蓄力压缩，史莱姆吐酸前接入膨胀与回弹，普通追击按 Skeleton/Slime 使用不同摇摆和弹跳风格。

验收标准：

- Unity Console 无项目级编译错误。
- 进入 Play Mode 后玩家身上存在 `CharacterAnimationController` 与 `PlayerAnimationDriver`。
- 玩家 Idle/Move/Attack/Dash/Hurt/Death 在实机中比第一轮更有重量和节奏。
- 骷髅冲锋与史莱姆吐酸前摇可明显区分。
- 不改变移动、攻击、闪避、受伤、死亡、敌人攻击判定等核心玩法逻辑。

验证记录：

- UnityMCP 脚本刷新编译通过，Console 仅曾出现 MCP 自身 WebSocket warning，无项目脚本 Error。
- 短 Play Mode 烟测确认 `Application.isPlaying=True`、开始菜单下 `Time.timeScale=0`、玩家动画组件存在。
- Play Mode 烟测期间 Console Error/Warning 为 0；退出后 `Application.isPlaying=False`、`Time.timeScale=1`。
- 仍缺完整人工玩法 QA：三段普攻观感、闪避爆发、受击/死亡重量、骷髅和史莱姆特殊动作的实机观感。

### 新增子任务：主角资源级动画 MVP

状态：smoke_verified_pending_gameplay_qa

内容：

- 盘点 KayKit 角色资源，确认 Adventurers 角色 FBX 内含约 152 个 Generic rig `AnimationClip`，Skeleton 角色 FBX 内含约 190 个 `AnimationClip`。
- 为主角 Knight 创建 `Assets/Resources/Animation/PlayerKnightResource.controller`，状态包含 Idle、Move、Attack1、Attack2、Attack3、Dash、Hurt、Death。
- 新增 `PlayerResourceAnimationDriver`，运行时查找 `KayKitVisual/Player_Knight_Model`，自动添加 Animator 并加载 `Resources/Animation/PlayerKnightResource`。
- `PlayerController.EnsureAnimationDriver()` 自动补齐 `PlayerResourceAnimationDriver`，不需要手动挂场景组件。
- Animator 负责骨骼级姿态变化，现有 `CharacterAnimationController` 继续作为外层程序动画叠加，保留手感增强。

验收标准：

- Unity Console 无项目级编译错误。
- 进入 Play Mode 后玩家存在 `PlayerResourceAnimationDriver`。
- `Player_Knight_Model` 运行时存在 Animator，并加载 `PlayerKnightResource`。
- Idle、Move、Attack1/2/3、Dash、Hurt、Death 能由桥接脚本按状态播放。
- 不改变现有移动、攻击、闪避、受伤、死亡判定逻辑。

验证记录：

- UnityMCP 刷新编译通过，Console 仅曾出现 MCP 自身 WebSocket warning，无项目脚本 Error。
- Play Mode 烟测确认 `bridge=True`、`model=True`、`animator=True`、`controller=PlayerKnightResource`。
- Play Mode 烟测期间 Console Error/Warning 为 0；退出后 `Application.isPlaying=False`、`Time.timeScale=1`。
- 仍缺完整人工玩法 QA：移动循环、三段攻击、闪避、受击和死亡的实际观感与程序动画叠加是否协调。

### 新增子任务：骷髅资源级动画接入

状态：smoke_verified_pending_visual_qa

内容：

- 针对用户截图反馈的“骷髅手臂僵硬”问题，为 Skeleton 敌人接入资源级骨骼动画。
- 新增 `EnemyResourceAnimationDriver`，Skeleton 运行时查找 `KayKitVisual/SkeletonEnemy_KayKit_Model`，自动添加 Animator 并加载 `Resources/Animation/SkeletonResource`。
- 创建 `Assets/Resources/Animation/SkeletonResource.controller`，状态包含 Idle、Move、Attack、Charge、Hurt、Death。
- `EnemyAI.EnsureAnimationDriver()` 自动补齐 `EnemyResourceAnimationDriver`。
- 普通攻击、骷髅冲锋前摇、受击、死亡会同步触发资源级动画，程序动画层继续作为整体压缩/摇摆叠加。

验收标准：

- Unity Console 无项目级编译错误。
- Skeleton prefab 实例化后存在 `EnemyResourceAnimationDriver`。
- `SkeletonEnemy_KayKit_Model` 运行时存在 Animator，并加载 `SkeletonResource`。
- 骷髅 Idle/Move/Attack/Charge/Hurt/Death 不再停留在僵硬手臂静态姿势。
- 不改变敌人 AI、攻击判定、伤害和生成逻辑。

验证记录：

- UnityMCP 刷新编译通过，Console 仅曾出现 MCP 自身 WebSocket warning，无项目脚本 Error。
- Play Mode 烟测实例化 `SkeletonEnemy_KayKit.prefab`，确认 `driver=True`、`model=True`、`animator=True`、`controller=SkeletonResource`。
- Play Mode 烟测期间 Console Error/Warning 为 0；退出后 `Application.isPlaying=False`、`Time.timeScale=1`。
- 仍缺人工视觉 QA：从游戏视角确认骷髅手臂、攻击、冲锋前摇是否自然。

## 风险控制规则

- 不直接删除或隐藏已有 UI，除非有新版等价内容并完成验证。
- 不改变已有输入语义，除非明确记录并验证。
- 所有 `Time.timeScale` 修改必须有恢复路径。
- 所有弹窗关闭必须恢复输入。
- 每完成一个阶段，更新本文件和 `progress.md`。

## 遇到的错误

| 错误 | 尝试次数 | 解决方案 |
|---|---:|---|
| 未找到 `Assets/Docs/2D_Roguelike_Game_Design_Document.md` | 1 | 已记录到 `findings.md`，后续确认是否需要重新生成或恢复 |

### 新增子任务：QA_Sandbox 与动画调参面板

状态：smoke_verified

内容：
- 新增独立 QA 场景 `Assets/Scenes/QA_Sandbox.unity`，包含玩家、摄像机、GameManager、CombatManager、UIManager、敌人生成点和 NavMesh。
- 新增运行时 `QASandboxController`，提供 F2 开关的 OnGUI 调试面板。
- 面板支持玩家伤害/治疗/击杀/加金币/升级/遗物面板/震屏/攻击/闪避/受击/死亡动画触发。
- 面板支持生成 Skeleton/Slime、选择最后敌人、清场、敌人攻击/冲锋/吐酸/受击/击杀测试。
- 面板支持对当前选中的 `CharacterAnimationController` 实时调整 Idle、Move、Hurt、Death 关键参数。
- `StartMenuManager` 已跳过 `QA_Sandbox`，避免 QA 场景进入时被开始菜单暂停。

验收记录：
- UnityMCP 刷新编译通过；Console 无项目级 Error/Warning，仅曾出现 MCP 自身 WebSocket warning。
- Play Mode 冒烟确认 `scene=QA_Sandbox`、`Application.isPlaying=True`、`Time.timeScale=1`、QA 面板存在。
- Play Mode 冒烟确认玩家存在 `CharacterAnimationController`、`PlayerResourceAnimationDriver`，`Player_Knight_Model` 存在 Animator。
- 通过 QA 面板生成 Skeleton 和 Slime 后，Skeleton 存在 `EnemyResourceAnimationDriver`、模型 Animator 和 `SkeletonResource` controller；Slime 存在程序动画层。
- 截图记录：`Assets/Screenshots/qa_sandbox_smoke.png`。

### 新增子任务：动画手感 QA 调参第一轮

状态：smoke_verified

内容：
- `CharacterAnimationController` 新增 `CharacterAnimationPreset`，区分 Player、Skeleton、Slime、Custom。
- 固化 Player/Skeleton/Slime 三套默认动作参数，让主角更有攻击/闪避爽感，Skeleton 外层程序动画更克制，Slime 更软弹。
- 新增攻击姿态、攻击位移、闪避拉伸、受击重量、死亡倒下强度等可调参数。
- `QASandboxController` 面板新增 Player/Skeleton/Slime 默认值按钮，并暴露新强度滑条。
- 兼容旧场景序列化：`Custom` 会在运行时根据 PlayerController 或 EnemyStats 类型推断默认 preset。

验收记录：
- UnityMCP 编译刷新通过，Console 无项目级 Error/Warning。
- Play Mode 验证 Player、Skeleton、Slime 三套默认值均正确应用。
- Play Mode 通过 QA 面板生成 Skeleton/Slime 后 Console Error/Warning 为 0。
- 截图记录：`Assets/Screenshots/qa_animation_tuning_defaults.png`。

### 新增子任务：升级祝福延后选择与暂停
状态：smoke_verified

内容：
- 升级后不再立刻暂停游戏。
- 右侧显示“祝福待选择”提示，玩家点击提示或按 U 后才暂停并打开祝福选择面板。
- 连续升级会累积待选祝福数量，进入选择后逐层处理，全部选完才恢复时间。
- 遗物奖励面板继续使用暂停恢复保护，避免奖励选择期间战斗继续流逝。

验收记录：
- QA_Sandbox Play Mode 验证：单次升级后 `Time.timeScale=1`，提示显示，选择面板不显示。
- 触发提示点击逻辑后 `Time.timeScale=0`，提示隐藏，祝福选择面板显示。
- 选择祝福后恢复 `Time.timeScale=1`。
- 连续升级验证：多层待选时游戏不暂停；进入选择后中途保持暂停，全部选完后恢复。
