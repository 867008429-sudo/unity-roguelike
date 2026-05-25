# 进度日志

## 2026-05-25

### 会话目标

使用 `planning-with-files-zh` 为项目规划下一阶段开发任务。

### 已完成

- 读取 `planning-with-files-zh` 的使用说明。
- 检查根目录是否已有规划文件，未发现旧的 `task_plan.md`、`findings.md`、`progress.md`。
- 读取 `Assets/Docs/Current_Work_Status_And_Optimization.md`。
- 尝试读取 `Assets/Docs/2D_Roguelike_Game_Design_Document.md`，当前未找到该文件。
- 创建 `task_plan.md`。
- 创建 `findings.md`。
- 创建 `progress.md`。
- 将 `task_plan.md` 的阶段 0 标记为 complete。

### 重要发现

- 当前项目下一阶段应先做稳定性和回归 QA，再做 UI 统一。
- 用户已明确反馈不希望 UI 和功能被删减，因此后续所有 UI 优化必须遵守“保留原功能、增量增强”的原则。

### 下一步

1. 进入阶段 1：稳定性修复与回归 QA。
2. 由用户手动触发 Unity 编译或 Play Mode，确认是否有项目级 Error。
3. 根据 QA 结果修复 `UIManager`、交互系统或相关 UI 问题。

### 新增开始界面

- 新增 `Assets/_Scripts/UI/StartMenuManager.cs`。
- 开始界面通过 `RuntimeInitializeOnLoadMethod` 自动生成，不需要手动挂到场景。
- 菜单包含：新游戏、读入存档、设置、退出游戏。
- 进入正式场景后菜单会把 `Time.timeScale` 保持为 `0`，点击新游戏后恢复为 `1`。
- 设置页先实现主音量滑条。
- 读入存档暂未接入实际存档系统，无存档时显示“暂无可用存档”。
- 中文菜单文字使用 legacy `Text`，避免当前 TMP 英文字体不支持中文。
- 本地已做脚本结构检查：`StartMenuManager.cs` 大括号平衡。
- 未触发 Unity MCP 编译或 Play Mode，避免再次出现 reconnect；需要用户在 Unity 中手动编译验证。

### 下载并学习 shadcn/ui

- 尝试 `git clone https://github.com/shadcn-ui/ui.git`，本机 `git` 因 DNS 无法解析 `github.com` 失败。
- 改用 GitHub zip 下载并解压成功。
- 源码目录整理为 `ReferenceRepos/shadcn-ui`。
- 阅读了 `README.md`、`components.json`、`app/globals.css`、`style.css` 主题文件，以及 Button、Card、Dialog、Tabs、Tooltip、Switch choice card、Stats card 示例。
- 创建学习笔记 `Assets/Docs/Shadcn_UI_Study_Notes.md`。
- 下一步可以把开始界面按钮改成 Unity 版 variant：Primary、Secondary、Outline、Danger。

### 开始界面 shadcn 风格第一轮落地

- 扩展 `Assets/_Scripts/UI/UITheme.cs`，新增 `UIButtonStyle`：Primary、Secondary、Outline、Ghost、Danger、Icon。
- 新增主题 token：Background、Foreground、Primary、Secondary、Muted、Danger、Border、Focus、Disabled。
- 新增 `UITheme.ApplyButton(...)`，统一按钮背景、文字颜色、描边和禁用态。
- 更新 `Assets/_Scripts/UI/StartMenuManager.cs`：
  - 新游戏使用 Primary。
  - 读入存档使用 Secondary。
  - 设置使用 Outline。
  - 退出游戏使用 Danger。
  - 返回使用 Ghost。
- 将设置页的主音量改为 Field 行结构，保留原主音量 Slider 功能。
- 本地静态检查通过：`UITheme.cs` 和 `StartMenuManager.cs` 大括号平衡；未发现旧 `CreateButton` 调用签名残留。
- 未触发 Unity Play Mode，避免 reconnect；仍需要用户在 Unity 中手动编译和进场景验证。

### 开始界面背景修正

- 将开始界面背景从半透明遮罩改为运行时生成的独立暗色背景，避免直接露出游戏场景。
- 将 `StartMenuCanvas.sortingOrder` 提高到 `30000`，减少被其他游戏 UI 盖住或混在一起的风险。
- 将 `UITheme.BackgroundColor` 改为完全不透明。
- 本地静态检查通过：`UITheme.cs` 和 `StartMenuManager.cs` 大括号平衡。

### 角色动画反馈第一轮

- 新增 `Assets/_Scripts/CharacterAnimationController.cs`，提供 Idle、Move、Attack、Dash、Hurt、Death 六类程序动画状态。
- 新增 `Assets/_Scripts/PlayerAnimationDriver.cs`，根据玩家移动、攻击、闪避、受伤、死亡自动驱动程序动画。
- 新增 `Assets/_Scripts/EnemyAnimationDriver.cs`，根据怪物巡逻/追击/攻击/受伤/死亡自动驱动程序动画。
- 更新 `PlayerStats`：新增 `OnDamaged` 事件，玩家受伤时触发 Hurt 动画；死亡时保留原输入锁定逻辑并触发 Death 动画。
- 更新 `PlayerController`：新增 `IsAttacking()` 查询，并在运行时自动补齐玩家动画组件。
- 更新 `EnemyAI`：运行时自动补齐怪物动画组件，并在每次攻击前摇触发 Attack 程序动画；原有攻击预警和伤害逻辑不变。
- 本地静态检查通过：新增/修改脚本大括号平衡。
- UnityMCP 已请求脚本刷新编译，Unity Console 最近 20 条 Error/Warning 为 0；未进入 Play Mode。

### HUD 左上角对齐修正

- 重新整理 `UIManager` 的 `ModernHudRoot` 为统一左上锚点布局，缩小整体尺寸并收紧边距。
- 将标题、HP、XP、Gold 三行改为同一列的网格式排布，避免内部居中锚点造成错位。
- 修正 XP 条的填充块锚点，避免向左溢出边框。
- 收紧金币数值区域并降低字号，减少右侧数字跑出容器的问题。
- 本地静态检查通过：`UIManager.cs` 大括号平衡。
- UnityMCP 刷新编译已恢复；Console 中仅有一条 MCP 自身的 WebSocket 初始化警告，无脚本 Error。

### HUD 字体舒适度修正

- 新增 `UITheme.HudTMPFont`，HUD 专用字体改用 TMP 自带 `LiberationSans SDF`，避免 MedievalSharp 用在小字号信息 UI 时发挤、发糊。
- 新增 `UITheme.ApplyHudTMPText(...)` 和 `UITheme.ApplyHudGoldEmphasis(...)`，让 HUD 文本和金币数字使用更克制的描边/阴影。
- 将 `ModernHudRoot` 内标题、等级、XP、Gold、图标标签切换到 HUD 字体。
- 将 `BufferedHealthBar` 的 HP 数值标签切换到 HUD 字体，并降低描边强度。
- 本地静态检查通过：`UITheme.cs`、`BufferedHealthBar.cs`、`UIManager.cs` 大括号平衡。
- UnityMCP 刷新编译已恢复；Console 中仅有一条 MCP 自身的 WebSocket 初始化警告，无脚本 Error。

### 场景精致度第一轮优化

- 在 `DungeonAdventurer` 场景中新增独立根对象 `ScenePolish_FirstPass`，用于承载本轮环境增强内容，避免混入玩法对象。
- 调整主方向光为偏冷柔光，降低平铺感；统一四个原有火把光源为暖色点光、软阴影和更合理的范围。
- 新增四角暖色火光池、两个冷色边缘补光，并开启轻微环境雾与冷色环境光。
- 使用 KayKit Dungeon 资源在场景边缘新增道具簇：箱子、桶、破桌、碎石、金币箱、烛台架、旗帜等，避开中心移动/战斗区域。
- 新增少量地面破损/污迹/格栅模型，打破大面积重复地砖。
- 第一版截图后发现边缘大件和假阴影垫过重，已进行第二轮微调：删除假阴影垫，缩小并外移部分旗帜、碎石和展示物。
- 场景已保存到 `Assets/Scenes/DungeonAdventurer.unity`。
- 截图输出：
  - `Assets/Screenshots/scene_polish_first_pass.png`
  - `Assets/Screenshots/scene_polish_second_pass.png`
- Console 当前仅有 MCP 自身 WebSocket 初始化警告，无项目脚本 Error。

### 场景小型出戏物修正

- 根据 Game View 标注，删除了 `ScenePolish_FirstPass/AccentLighting` 下 4 个可见 `TorchEmber_*` 小球。
- 保留 `WarmTorchPool_*` 点光源，不影响火把暖光氛围。
- 场景已重新保存到 `Assets/Scenes/DungeonAdventurer.unity`。
- 新截图输出：`Assets/Screenshots/scene_polish_no_embers.png`。
- Unity Console 当前 Error/Warning 为 0。

### 金币拾取物视觉修正

- 删除场景布景中的 `SouthEast_CoinSmall` 装饰金币，以及带凸起小石子的 `FloorDirt_*` 地面装饰，避免在俯视镜头下形成棕色小点。
- 更新 `GoldPickup`：默认金币拾取物会统一成金色自发光材质、略微放大，并加入轻微上下浮动。
- 更新 `EnemyStats.CreateDefaultGold(...)` 和 `DestructiblePropInteractable.SpawnGold()`：默认生成金币的高度、比例和颜色更明确，避免看起来像地面污点。
- 本地静态检查通过：`GoldPickup.cs`、`EnemyStats.cs`、`DestructiblePropInteractable.cs` 大括号平衡。
- UnityMCP 刷新编译已恢复；Console 中仅有 MCP 自身 WebSocket 初始化警告，无脚本 Error。

### TorchVisual 与新增布景碰撞修正

- 删除旧 `Lights/Torch_*/TorchVisual` 下 5 个悬浮橙色球体，只保留实际灯光，避免火把光源占位物穿帮。
- 为 `ScenePolish_FirstPass/EdgeSetDressing` 下 9 个新增非旗帜大件自动添加 BoxCollider，避免玩家直接穿过新增箱子、桶、破桌、碎石、金币箱、烛台架等。
- 旗帜类装饰保持无碰撞，避免贴边布景卡住移动。
- 场景已保存到 `Assets/Scenes/DungeonAdventurer.unity`。
- 新截图输出：`Assets/Screenshots/scene_no_torch_visuals_colliders.png`。
- Unity Console 当前 Error/Warning 为 0。

### 新增布景碰撞体 QA 与贴合修正

- 用户反馈上一轮 BoxCollider 过大、和模型不贴合。
- 通过 UnityMCP 在编辑态检查并确认 `Application.isPlaying = false`，确保修改会持久保存。
- 移除 `ScenePolish_FirstPass/EdgeSetDressing` 下 9 个粗 BoxCollider。
- 为新增非旗帜大件按 MeshFilter 添加 10 个 MeshCollider，让碰撞轮廓跟随模型网格，不再使用单个大 bounds 盒。
- 旗帜类继续无碰撞，避免边缘装饰卡移动。
- QA 检查：
  - 渲染 bounds 与碰撞 bounds 比例均为 `1.00`。
  - 玩家胶囊体四向探针检查：大多数大件周边清空；贴墙的 `NorthEast_RubbleLarge` 和 `SouthEast_ChestGold` 各有 1 个方向被墙/邻近物阻挡，符合贴边布景预期。
- 场景已保存到 `Assets/Scenes/DungeonAdventurer.unity`。
- Unity Console 当前 Error/Warning 为 0。

### 血瓶拾取物视觉修正

- 将默认血瓶掉落从单个红色圆柱体改为可读的瓶子结构：瓶身、瓶颈、木塞、液体内芯、前侧高光和轻微红色微光。
- `EnemyStats.CreateDefaultPotion(...)` 现在只生成拾取根物体，具体视觉由 `HealthPotion` 在运行时自动补齐，保留现有拾取、回血、浮动和生命周期逻辑。
- 若后续接入正式 `potionPickupPrefab`，已有 `HealthPotion` 仍可复用同一拾取逻辑，不需要推翻当前接口。
- UnityMCP 脚本刷新编译已恢复 ready；Console 仅有 MCP WebSocket 初始化 warning，未发现项目脚本 Error。

### 新手按键说明第一版

- 在 `UIManager` 中新增 `ControlsHelpPanel`，首次真正开始游戏后自动显示 14 秒，不暂停游戏。
- 面板说明移动、攻击、闪避、交互、升级选择、暂停、死亡重开等当前实际按键。
- 新增 `F1` 开关说明面板；首次显示后写入 `PlayerPrefs`，后续不会每次自动打扰玩家。
- 保持开始菜单、HUD、升级/遗物选择、暂停与死亡 UI 原有功能不变。
- UnityMCP 脚本刷新编译已恢复 ready；Console 仅有 MCP WebSocket 初始化 warning，未发现项目脚本 Error。

### ESC 暂停菜单功能扩展

- 将原本只有提示文案的 `PausePanel` 扩展为主菜单、设置、退出确认三页结构。
- 暂停主菜单新增“继续游戏 / 设置 / 退出游戏”按钮，保留 ESC 快速返回游戏。
- 设置页先接入主音量 Slider，并补充 F1 按键说明入口提示。
- 退出游戏加入确认页；在正式构建中调用 `Application.Quit()`，在 Unity 编辑器中会停止 Play Mode。
- 本地静态检查通过：`UIManager.cs` 大括号平衡；UnityMCP 刷新编译后 Console 无 Error/Warning。

### 战斗手感与敌人差异化第一轮

- 修正震屏实现：`CameraFollow` 现在在相机跟随位置上叠加 shake offset，避免原先 `LateUpdate` 把 `CombatManager` 的震屏覆盖掉。
- `CombatManager` 的震屏调用优先转交给 `CameraFollow.AddShake(...)`，保留无跟随相机时的旧 fallback。
- 新增 `EnemyProjectile`，用于敌人远程弹体命中玩家、播放命中特效并自动销毁。
- `EnemyAI` 新增普通怪特殊行为：骷髅会在中距离蓄力冲锋，史莱姆会在中距离吐出远程弹体。
- `WaveManager` 按敌人类型配置特殊行为：Skeleton = 冲锋，Slime = 投射物；Boss 仍保留原 Boss 技能，不混入普通怪特殊行为。
- 本地静态检查通过：`CameraFollow.cs`、`CombatManager.cs`、`EnemyAI.cs`、`WaveManager.cs`、`EnemyProjectile.cs` 大括号平衡；UnityMCP 刷新编译后 Console 无 Error/Warning。

### 战斗节奏与远程威胁第二轮

- 新增 `EnemyHazardZone`，用于史莱姆弹体落地后生成短暂酸液区，形成走位压力。
- 调整 `EnemyProjectile`：弹体现在朝目标落点飞行，命中玩家会直接造成伤害；未命中时在落点生成酸液池。
- 调整技能引入节奏：第 1 波保持基础怪；第 2 波提示并启用史莱姆吐酸；第 3 波提示并启用骷髅冲锋；精英怪仍可提前获得对应特殊行为。
- `WaveManager` 的波次提示现在会在第 2 / 第 3 波明确告诉玩家新威胁。
- 本地静态检查通过：`EnemyProjectile.cs`、`EnemyHazardZone.cs`、`EnemyAI.cs`、`WaveManager.cs` 大括号平衡。
- UnityMCP 刷新编译后 Console 无 Error/Warning；短 Play Mode smoke test 进入后无 Error/Warning，截图命令触发 MCP 断连并自动退出，已确认 `Application.isPlaying=False`、`Time.timeScale=1`。

### 武器/流派雏形第一轮

- 新增三条 build 层数：`critBuildLevel`、`burnBuildLevel`、`lightningBuildLevel`，作为后续正式技能树/武器系统的轻量接口。
- 暴击流：升级选项改为 `Crit Path`，暴击命中会产生更明显的金色爆发；层数达到 2 后会对附近敌人造成小范围暴击溅射。
- 燃烧流：升级选项改为 `Burn Path`，点燃目标时会向附近敌人扩散较弱燃烧，层数越高扩散数量和范围越好。
- 闪电流：升级选项改为 `Lightning Path`，冲击波的范围和伤害随层数提高，层数达到 2 前控制连锁数量，避免早期过强。
- 遗物选项同步标注并强化三条流派：Assassin Sigil / Inferno Sigil / Thunder Crown 分别提供对应 +2 路线层数。
- Build 信息面板新增 Crit / Burn / Lightning 层数显示，方便后续 QA 和调参。
- 本地静态检查通过：`PlayerStats.cs`、`PlayerController.cs`、`UIManager.cs` 大括号平衡；UnityMCP 刷新编译通过，短 Play Mode smoke test 进入/退出后 Console 无 Error/Warning。

### 升级选项简体中文化

- 将普通升级选项标题和描述改为简体中文，保留原有数值和效果逻辑不变。
- 将遗物选项标题和描述改为简体中文，并保留暴击/燃烧/闪电三条流派的识别标签。
- 将升级面板标题改为“选择祝福”，遗物面板标题改为“选择遗物”，提示文案改为中文。
- `UITheme` 新增 TMP 中文字体兜底：优先使用 FusionPixel 中文字体运行时生成 TMP FontAsset，避免升级/遗物 TMP 文本出现缺字。
- 本地静态检查通过：`UIManager.cs`、`UITheme.cs` 大括号平衡；UnityMCP 刷新编译通过，Console 仅有 MCP WebSocket warning，无项目脚本 Error。

### 升级面板中文乱码修正

- 用户反馈升级选项正文仍显示为方块乱码，确认是 TMP 字体资产缺字导致。
- 将升级/遗物面板的标题、提示和选项正文从 TMP 文本切换为 Legacy `Text`，复用项目已有 `FusionPixelZhHans` 中文字体。
- 新增 `ConfigureChoiceText(...)`，为升级/遗物选项设置自动缩放、换行和富文本支持，避免中文长句溢出。
- Play Mode 验证：强制触发升级面板后，`UpgradePanel` 中 legacy 文本均使用 `FusionPixelZhHans`，示例文本“选择祝福 / 战士之力 / 迅捷出手 / 战斗步法”等可被正确绑定到中文字体。
- Unity Console 进入/退出 Play Mode 后 Error/Warning 为 0。

### 局外成长第一版

- 新增 `MetaProgression`，使用 `PlayerPrefs` 保存魂石、生命强化、攻击训练、防御训练等级。
- 玩家死亡结算时按击杀和本局等级计算魂石奖励，并写入总魂石；死亡面板显示本局魂石获得和总魂石。
- `PlayerStats.Awake()` 读取局外成长，开局应用最大生命、攻击、防御的小额加成。
- 开始菜单新增“成长”入口和 `ProgressionPanel`，可花魂石升级生命强化、攻击训练、防御训练，单项上限 5 级。
- Play Mode 验证：开始菜单“成长 / 局外成长 / 魂石 / 生命强化”等文本均绑定 `FusionPixelZhHans`；临时发放魂石并购买生命强化成功，测试后已将测试用 PlayerPrefs 重置为 0。
- Unity Console 进入/退出 Play Mode 后 Error/Warning 为 0。

### 战斗爽感第一轮

- 扩展 `VisualEffectsManager`：新增玩家专用 `PlayPlayerSlash(...)`，三段普攻使用不同长度、宽度、颜色、持续时间；新增 `PlayGroundPulse(...)` 复用 slash 池做地面脉冲。
- 扩展 `CombatManager`：新增带 `comboStep` 的 `TriggerPlayerAttackImpact(...)` 重载，让 2/3 段普攻有额外震屏和地面反馈。
- 更新 `PlayerController`：
  - 普攻前摇/后摇按 1/2/3 段略有差异，第三段更重。
  - 斩击 VFX 使用玩家专用三段表现；已有流派层数时会轻微偏向能量色。
  - 暴击、燃烧、闪电触发时增加对应金色/火色/蓝色地面脉冲。
  - 闪避起步增加蓝色脉冲，闪避窗口内规避伤害会触发“完美闪避”、蓝色爆发，并小幅返还攻击/闪避冷却。
- 更新 `PlayerStats.TakeDamage()`：保留原闪避免伤逻辑，在免伤分支尝试触发完美闪避奖励。
- 扩展 `DamageTextPool` / `DamageTextPopup`：新增通用短文本飘字，用于“完美闪避”。
- 本地静态检查通过：`VisualEffectsManager.cs`、`PlayerController.cs`、`PlayerStats.cs`、`CombatManager.cs`、`DamageTextPool.cs`、`DamageTextPopup.cs` 大括号平衡。
- UnityMCP 刷新编译成功；期间 MCP WebSocket warning 自动恢复，Console 无项目 Error。
- 短 Play Mode 烟测：进入 `DungeonAdventurer` 后 `Application.isPlaying=True`、开始菜单下 `Time.timeScale=0`；退出后 `Application.isPlaying=False`、`Time.timeScale=1`；Console Error/Warning 为 0。

### 规划状态同步

- 根据当前进度记录同步更新 `task_plan.md`，避免计划状态落后于实际开发。
- 将当前阶段和阶段 1 从 `pending` 更新为 `in_progress`，并补充状态说明：已有编译刷新与短 Play Mode 烟测，但仍缺完整人工回归。
- 将“开始界面”更新为 `smoke_verified`，记录已验证进入场景后开始菜单暂停、退出后 `Time.timeScale=1`，并标注仍建议逐项点击回归。
- 将“开始界面按钮 variant 与设置 Field 化”更新为 `compile_verified_pending_button_qa`，记录已通过静态检查和编译刷新，仍待按钮行为与主音量 Slider 人工 QA。
- 将“角色动画反馈第一轮”更新为 `compile_verified_pending_playmode_qa`，记录已通过静态检查和编译刷新，仍待 Play Mode 可见反馈逐项验证。
- 将“战斗爽感第一轮”更新为 `smoke_verified_pending_gameplay_qa`，记录已通过编译和短 Play Mode 烟测，仍待三段普攻、流派触发、完美闪避的完整实机 QA。

### Git 仓库初始化

- 在项目根目录初始化本地 Git 仓库。
- 新增 Unity 适用 `.gitignore`，忽略 `Library/`、`Temp/`、`Logs/`、`UserSettings/`、`ReferenceRepos/`、`TempAssets/` 等本地缓存、临时资源和参考源码目录。
- 修正 `.gitignore` 中 `/Obj/` 为根目录匹配，避免误忽略 `Assets/_ImportedArt/KayKit/Dungeon/obj/` 美术资源目录。
- 检查未发现超过 90MB 的待提交单文件。
- 创建初始提交 `035da4c Initial Unity project baseline`，包含 `Assets/`、`Packages/`、`ProjectSettings/`、规划文件和 `.gitignore`。
- 将默认分支改名为 `main`。
- 已设置 GitHub 远端 `origin = https://github.com/867008429-sudo/unity-roguelike.git`。
- 已成功推送 `main` 分支，并建立本地 `main` 对 `origin/main` 的 upstream 跟踪。

### 程序动画增强第二轮

- 新建功能分支 `feature/program-animation-feel-pass`，用于集中开发角色程序动画爽感增强。
- 重写 `CharacterAnimationController` 的动画曲线：
  - Idle 加入呼吸、轻微浮动和材质根节点的柔和回正。
  - Move 加入步频、弹跳、身体前倾/侧倾、启动和停止惯性。
  - Attack 改为蓄力、挥出、回弹三段式曲线，并按 combo 1/2/3 区分轻击、横扫、重击。
  - Dash 改为起步压缩、中段拉伸、结束刹车回正。
  - Hurt 加入闪白、后仰、短暂失衡和回正。
  - Death 改为倒下、收缩、淡出三段式程序动画。
- `PlayerAnimationDriver` 不再在攻击开始时重复触发 `PlayAttack`，避免和 `PlayerController` 的 combo 时序抢控制；它现在只负责移动上下文、闪避、受击和死亡。
- `PlayerController` 在 comboStep 计算完成后直接调用 `CharacterAnimationController.PlayAttack(facingDirection, comboStep)`，并移除攻击过程中旧的直接 `visualRoot` 扭动调用；第三段攻击临时强化拖尾。
- `EnemyAnimationDriver` 根据 `EnemyStats.EnemyType` 设置 Skeleton/Slime 动作口味，让普通追击有不同的摇摆和弹跳。
- `EnemyAI` 的骷髅冲锋前摇接入 `PlayChargeWindup(...)`，史莱姆吐酸前摇接入 `PlaySpitWindup(...)`。
- 本地静态检查通过：`CharacterAnimationController.cs`、`PlayerAnimationDriver.cs`、`EnemyAnimationDriver.cs`、`PlayerController.cs`、`EnemyAI.cs` 大括号平衡，新增方法引用可解析。
- UnityMCP 刷新编译成功；期间仅出现 MCP 自身 WebSocket warning，无项目脚本 Error。
- 短 Play Mode 烟测：进入后 `Application.isPlaying=True`、开始菜单下 `Time.timeScale=0`、玩家存在 `CharacterAnimationController` 和 `PlayerAnimationDriver`；Console Error/Warning 为 0；退出后 `Application.isPlaying=False`、`Time.timeScale=1`。
- 尚未完成完整实机玩法 QA，需要后续人工确认 Idle/Move/三段攻击/闪避/受击/死亡，以及骷髅冲锋和史莱姆吐酸前摇的实际观感。

### 主角资源级动画 MVP

- 通过 Unity AssetDatabase 盘点 KayKit FBX 内嵌动画：
  - Adventurers 主角角色 `Barbarian/Knight/Rogue/RogueHooded/Mage` 均为 Generic rig，单模型约 152 个 `AnimationClip`。
  - Skeleton 角色模型为 Generic rig，单模型约 190 个 `AnimationClip`。
  - 可用主角基础 clips 包括 `Idle`、`Running_A`、`1H_Melee_Attack_Chop`、`1H_Melee_Attack_Slice_Horizontal`、`2H_Melee_Attack_Chop`、`Dodge_Forward`、`Hit_A`、`Death_A`。
- 确认当前正式场景玩家视觉为 `Player/KayKitVisual/Player_Knight_Model`，由 `Assets/_ImportedArt/KayKit/Adventurers/Characters/fbx/Knight.fbx` 实例化而来，之前没有 Animator。
- 新增 `Assets/_Scripts/PlayerResourceAnimationDriver.cs`：
  - 运行时查找 `KayKitVisual/Player_Knight_Model`。
  - 自动添加 Animator。
  - 从 `Resources.Load<RuntimeAnimatorController>(\"Animation/PlayerKnightResource\")` 加载控制器。
  - 根据玩家移动、攻击、闪避、受击、死亡播放资源级骨骼动画。
- 更新 `PlayerController.EnsureAnimationDriver()`，自动补齐 `PlayerResourceAnimationDriver`。
- 创建 `Assets/Resources/Animation/PlayerKnightResource.controller`，状态映射：
  - `Idle -> Idle`
  - `Move -> Running_A`
  - `Attack1 -> 1H_Melee_Attack_Chop`
  - `Attack2 -> 1H_Melee_Attack_Slice_Horizontal`
  - `Attack3 -> 2H_Melee_Attack_Chop`
  - `Dash -> Dodge_Forward`
  - `Hurt -> Hit_A`
  - `Death -> Death_A`
- UnityMCP 刷新编译通过；期间仅出现 MCP 自身 WebSocket warning，无项目脚本 Error。
- 短 Play Mode 烟测：进入后玩家 `bridge=True`、`model=True`、`animator=True`、`controller=PlayerKnightResource`，Console Error/Warning 为 0；退出后 `Application.isPlaying=False`、`Time.timeScale=1`。
- 当前仍需人工实机 QA，重点看资源级骨骼动画和外层程序动画叠加是否自然，特别是三段攻击和闪避。

### 骷髅手臂僵硬修正

- 用户截图反馈 Skeleton 手臂不自然、僵硬，判断原因为 Skeleton 只有外层 `CharacterAnimationController` 程序动画，模型骨骼本身没有接 Animator，手臂停留在静态姿态。
- 新增 `Assets/_Scripts/EnemyResourceAnimationDriver.cs`：
  - 当前只对 `EnemyStats.EnemyType.Skeleton` 启用资源级 Animator。
  - 运行时查找 `KayKitVisual/SkeletonEnemy_KayKit_Model`。
  - 自动添加 Animator。
  - 从 `Resources.Load<RuntimeAnimatorController>(\"Animation/SkeletonResource\")` 加载控制器。
  - 根据移动、攻击、冲锋前摇、受击、死亡播放骨骼动画。
- 更新 `EnemyAI`：
  - 自动补齐 `EnemyResourceAnimationDriver`。
  - 普通攻击时调用 `resourceAnimationDriver.PlayAttack()`。
  - 骷髅冲锋前摇调用 `resourceAnimationDriver.PlayChargeWindup()`。
  - 史莱姆吐酸保留兼容调用，但当前 Slime 不启用资源 Animator。
- 创建 `Assets/Resources/Animation/SkeletonResource.controller`，状态映射：
  - `Idle -> Idle_B`
  - `Move -> Walking_D_Skeletons`
  - `Attack -> 1H_Melee_Attack_Slice_Horizontal`
  - `Charge -> 1H_Melee_Attack_Jump_Chop`
  - `Hurt -> Hit_A`
  - `Death -> Death_C_Skeletons`
- UnityMCP 刷新编译通过；期间仅出现 MCP 自身 WebSocket warning，无项目脚本 Error。
- Play Mode 烟测实例化 `Assets/_Prefabs/KayKit/SkeletonEnemy_KayKit.prefab`：`driver=True`、`model=True`、`animator=True`、`controller=SkeletonResource`；Console Error/Warning 为 0；退出后 `Application.isPlaying=False`、`Time.timeScale=1`。
- 后续需要人工从 Game 视角确认手臂自然度、攻击动作和冲锋前摇是否符合预期。

### QA_Sandbox 与动画调参面板

- 新增 `Assets/_Scripts/QA/QASandboxController.cs`，运行时提供 F2 可开关的 QA 面板。
- 面板支持玩家伤害、治疗、击杀、加金币、强制升级、打开遗物面板、震屏、攻击/闪避/受击/死亡动画触发。
- 面板支持生成 Skeleton/Slime、选择最后生成敌人、清场、触发敌人攻击/冲锋/吐酸/受击/击杀。
- 面板支持对当前选中的 `CharacterAnimationController` 实时调整 Idle/Move/Hurt/Death 关键参数。
- 新增 `Assets/_Scripts/Editor/QASandboxSceneBuilder.cs`，可通过 `QASandboxSceneBuilder.BuildQASandbox()` 重建 `Assets/Scenes/QA_Sandbox.unity`。
- 生成 `Assets/Scenes/QA_Sandbox.unity` 与 `Assets/Scenes/QA_Sandbox/NavMesh.asset`，QA 场景包含玩家、摄像机、GameManager、CombatManager、UIManager、敌人生成点和 NavMesh。
- 更新 `StartMenuManager`，让 `QA_Sandbox` 跳过开始菜单暂停逻辑。
- 处理一次构建副产物：首次 NavMesh 在保存场景前生成，Unity 留下 `Assets/1111*` 临时文件；已清理并修正为先保存 QA 场景再 BuildNavMesh。
- UnityMCP 编译刷新通过，Console 无项目级 Error/Warning，仅出现过 MCP 自身 WebSocket warning。
- Play Mode 冒烟：`scene=QA_Sandbox`、`Application.isPlaying=True`、`Time.timeScale=1`、QA 面板存在。
- Play Mode 冒烟：玩家存在 `CharacterAnimationController`、`PlayerResourceAnimationDriver`，`Player_Knight_Model` 存在 Animator。
- 通过 QA 面板生成 Skeleton/Slime 后，Skeleton 存在 `EnemyResourceAnimationDriver`、模型 Animator 和 `SkeletonResource` controller；Slime 存在程序动画层。
- 截图保存到 `Assets/Screenshots/qa_sandbox_smoke.png`；退出 Play Mode 后确认 `Application.isPlaying=False`、`Time.timeScale=1`。

### 动画手感 QA 调参第一轮

- 更新 `CharacterAnimationController`，新增 `CharacterAnimationPreset`：`Player`、`Skeleton`、`Slime`、`Custom`。
- 固化三套默认动作参数：
  - Player：更克制的 Idle 呼吸、更明显的移动倾斜、更强攻击姿态和闪避拉伸。
  - Skeleton：降低外层压缩和弹跳，保留轻微骨架摇摆，避免和资源级骨骼动画互相打架。
  - Slime：增强呼吸、弹跳、受击重量和软体拉伸，突出和 Skeleton 的差异。
- 新增攻击姿态、攻击位移、闪避拉伸、受击重量、死亡倒下强度等可调参数，补足 QA 面板只能调 Idle/Move 的不足。
- 更新 `QASandboxController`：面板新增 `Player Defaults`、`Skeleton Defaults`、`Slime Defaults` 按钮，并暴露新强度滑条。
- 兼容旧场景序列化：若旧组件的 preset 为 `Custom`，运行时会根据 `PlayerController` 或 `EnemyStats.enemyType` 自动套入对应默认值。
- 重建并验证 `QA_Sandbox`，Play Mode 中确认默认值：
  - Player：`preset=Player`、`idle=0.038`、`sway=10.5`、`attack=1.08`、`dash=1.12`
  - Skeleton：`preset=Skeleton`、`idle=0.025`、`sway=7.2`、`attack=0.92`、`dash=0.85`
  - Slime：`preset=Slime`、`idle=0.055`、`sway=4.4`、`attack=1.18`、`dash=1.25`
- UnityMCP 编译刷新通过；Play Mode 生成 Skeleton/Slime 后 Console Error/Warning 为 0。
- 截图保存到 `Assets/Screenshots/qa_animation_tuning_defaults.png`；退出 Play Mode 后确认 `Application.isPlaying=False`、`Time.timeScale=1`。

### 升级祝福延后选择与暂停
- 根据用户反馈，将“升级后立刻暂停选祝福”改为“升级后右侧弹出祝福待选择提示，玩家点击提示或按 U 后再暂停并进入祝福选择”。
- 更新 `Assets/_Scripts/UIManager.cs`：新增 `UpgradeReadyPrompt` 侧边提示；升级时只累积 `pendingUpgradeChoices` 并显示提示，不立即调用 `ShowUpgradeChoices()`；点击提示后进入选择并暂停。
- 连续升级时保持待选数量，进入选择后逐层选择，全部选完再恢复 `Time.timeScale=1`。
- 遗物选择继续使用同一套暂停恢复保护，避免奖励面板期间战斗继续流逝。
- UnityMCP 刷新编译通过；Console 仅曾出现 MCP 自身 WebSocket warning，最终 Play Mode 验证后 Error/Warning 为 0。
- QA_Sandbox Play Mode 验证：单次升级后 `Time.timeScale=1`，`UpgradeReadyPrompt` 显示，`UpgradePanel` 不显示；触发提示点击逻辑后 `Time.timeScale=0`，提示隐藏，`UpgradePanel` 显示；选择祝福后恢复 `Time.timeScale=1`。
- 连续升级验证：多层待选时游戏不暂停且提示显示；进入选择后中途保持 `Time.timeScale=0`，全部选完后恢复 `Time.timeScale=1`。

### 敌人资源级动画后续方向记录
- 用户确认骷髅已经开始接入资源级动画，后续可以继续补：史莱姆吐酸/受击/死亡、Boss 动作、精英怪颜色/动画差异、敌人攻击前摇统一规范。
- 已将这组方向整理为 `task_plan.md` 中的后续任务“敌人资源级动画第二轮与攻击前摇规范”。
- 建议下一步优先做史莱姆：它和骷髅差异最大，补完吐酸前膨胀、受击回弹、死亡软体坍缩后，敌人类型辨识度会立刻上一个台阶。
