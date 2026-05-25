# shadcn/ui 学习笔记与 Unity UI 借鉴方案

更新时间：2026-05-25

参考源码位置：

- `ReferenceRepos/shadcn-ui`
- 重点目录：`ReferenceRepos/shadcn-ui/apps/v4`

## 一、下载结果

`git clone` 因本机 DNS 无法解析 `github.com` 失败，已改用 GitHub zip 包下载并解压。

当前源码目录：

`ReferenceRepos/shadcn-ui`

该目录不在 `Assets` 下，因此 Unity 不会把前端源码当成游戏资源导入。

## 二、shadcn/ui 的核心设计思想

shadcn/ui 不是传统意义上的 UI 库，而是一套可复制、可修改、可组合的组件范式。

它的核心特点：

- 基础组件很小：Button、Card、Dialog、Tabs、Tooltip、Switch 等都职责单一。
- 组件由语义部件组合：例如 Card 拆成 Header、Title、Description、Content、Footer、Action。
- 样式靠统一 token 管理：background、foreground、card、primary、muted、border、ring、destructive 等。
- 组件有明确 variant：按钮有 default、outline、secondary、ghost、destructive、link、icon 等。
- 组件有明确 size：按钮、输入框、图标按钮都有稳定尺寸。
- 状态清晰：hover、active、disabled、focus ring、dark mode 都有统一规则。
- 默认不花哨，强调信息清晰、层级干净、可维护。

## 三、可借鉴到当前 Unity 项目的部分

### 1. 主题 token 化

shadcn/ui 的全局样式中大量使用 CSS 变量：

- `background`
- `foreground`
- `card`
- `card-foreground`
- `primary`
- `primary-foreground`
- `secondary`
- `muted`
- `muted-foreground`
- `accent`
- `destructive`
- `border`
- `input`
- `ring`

对应到 Unity，可以把 `UITheme.cs` 扩展成更完整的 token：

- `BackgroundColor`
- `ForegroundColor`
- `PanelColor`
- `PanelTextColor`
- `PrimaryColor`
- `PrimaryTextColor`
- `SecondaryColor`
- `MutedColor`
- `MutedTextColor`
- `AccentColor`
- `DangerColor`
- `BorderColor`
- `FocusColor`
- `DisabledColor`

这样以后不用在 `UIManager.cs` 里到处写临时颜色。

### 2. Button variant

shadcn/ui 的按钮不是只有一种样式，而是按用途区分：

- default：主操作
- outline：次级操作
- secondary：普通辅助操作
- ghost：弱操作
- destructive：危险操作
- link：文本链接
- icon：纯图标按钮

对应到当前游戏：

- 新游戏：PrimaryButton
- 读入存档：SecondaryButton
- 设置：OutlineButton
- 退出游戏：DangerButton
- 返回：GhostButton
- 关闭弹窗：IconButton

建议新增或扩展：

- `UITheme.ApplyButton(Image image, ButtonStyle style)`
- `ButtonStyle.Primary`
- `ButtonStyle.Secondary`
- `ButtonStyle.Outline`
- `ButtonStyle.Ghost`
- `ButtonStyle.Danger`
- `ButtonStyle.Icon`

### 3. Card 组件拆分

shadcn/ui 的 Card 通常拆成：

- Card
- CardHeader
- CardTitle
- CardDescription
- CardContent
- CardFooter
- CardAction

对应到 Unity：

- 升级选项卡：标题、描述、快捷键、稀有度/类型标签。
- 遗物选项卡：遗物名、效果说明、图标、确认区域。
- 死亡结算卡：标题、统计、操作提示。
- 设置面板：设置项列表、说明、控制项。

建议不要把所有内容都直接塞到一个 Text 里。后续升级/遗物面板可以做成：

- `ChoiceCardRoot`
- `ChoiceCardHeader`
- `ChoiceCardTitle`
- `ChoiceCardDescription`
- `ChoiceCardShortcutBadge`

### 4. Dialog 结构

shadcn/ui 的 Dialog 常见结构：

- Dialog
- DialogTrigger
- DialogContent
- DialogHeader
- DialogTitle
- DialogDescription
- DialogFooter
- DialogClose

对应当前项目：

- 暂停面板
- 设置面板
- 死亡面板
- 确认退出面板

建议每个弹窗都统一：

- 标题区
- 描述区
- 内容区
- 底部按钮区
- 遮罩层
- 打开/关闭动画
- TimeScale 管理

尤其是退出游戏，后续可以从直接退出升级为确认 Dialog：

- 标题：退出游戏
- 描述：当前进度尚未保存
- 按钮：取消 / 退出

### 5. Tabs

shadcn/ui 的 Tabs 适合组织设置页或图鉴页。

当前项目后续设置界面可以分成：

- 音频
- 画面
- 操作
- 游戏

对应 Unity：

- `SettingsTabs`
- `AudioTab`
- `VideoTab`
- `ControlsTab`
- `GameplayTab`

当前开始界面的设置页只有主音量滑条，后续可以按 Tabs 扩展。

### 6. Tooltip

shadcn/ui 的 Tooltip 思路非常适合游戏 UI：

- 图标按钮只显示图标。
- 鼠标悬停显示说明。
- 对危险操作给出解释。
- 对未解锁功能说明原因。

当前游戏可用场景：

- 设置按钮图标。
- 关闭按钮。
- 遗物效果说明。
- 属性图标说明。
- 存档不可用时说明“暂无可用存档”。

### 7. Choice Card / Switch Card

shadcn/ui 示例中有一种 Switch choice card：左边标题和描述，右边开关。

可用于：

- 设置页的开关项。
- 自动拾取金币。
- 显示伤害数字。
- 屏幕震动。
- 全屏模式。
- 显示交互提示。

Unity 实现时可以做成：

- 左侧：Title + Description
- 右侧：Toggle
- 整行可点击
- Hover / Focus 高亮

## 四、对当前开始界面的具体改进建议

当前开始界面已经包含：

- 新游戏
- 读入存档
- 设置
- 退出游戏

下一步可以借鉴 shadcn/ui 做以下优化：

### 1. 菜单按钮分级

- 新游戏：主按钮，高亮。
- 读入存档：次级按钮。
- 设置：outline 按钮。
- 退出游戏：danger 或 ghost-danger 按钮。

### 2. 设置页改成 Field 行

当前设置页只有主音量，可以改成：

- 主音量：Slider
- 音效音量：Slider
- 屏幕震动：Toggle
- 显示伤害数字：Toggle
- 返回：GhostButton

### 3. 读档按钮状态

当前是点击后显示“暂无可用存档”。后续可改成：

- 无存档：按钮仍显示，但文本旁加小提示。
- Hover tooltip：暂无可用存档。
- 有存档：显示最近存档时间、等级、波次。

### 4. 退出游戏确认弹窗

不要直接退出，改成 Dialog：

- 标题：退出游戏
- 描述：确定要退出吗？
- 操作：取消 / 退出

## 五、对当前 Unity UITheme 的建议改造

建议新增枚举：

```csharp
public enum UIButtonStyle
{
    Primary,
    Secondary,
    Outline,
    Ghost,
    Danger,
    Icon
}
```

建议新增方法：

```csharp
public static void ApplyButton(Image image, Text text, UIButtonStyle style)
public static void ApplyPanel(Image image, UIPanelStyle style)
public static void ApplyLabel(Text text, UILabelStyle style)
public static void ApplyTMPLabel(TMP_Text text, UILabelStyle style)
```

建议新增 token：

```csharp
public static readonly Color BackgroundColor;
public static readonly Color ForegroundColor;
public static readonly Color PanelColor;
public static readonly Color PrimaryColor;
public static readonly Color SecondaryColor;
public static readonly Color MutedColor;
public static readonly Color DangerColor;
public static readonly Color BorderColor;
public static readonly Color FocusColor;
```

## 六、适合优先落地的任务

### P0：开始界面按钮 variant

目标：

- 新游戏、读入存档、设置、退出游戏有不同视觉层级。

### P1：设置页 Field 化

目标：

- 主音量不再是孤立控件，而是一行标准设置项。

### P2：统一弹窗结构

目标：

- 暂停、死亡、设置、退出确认都使用统一 Dialog 结构。

### P3：Tooltip 系统

目标：

- 为图标按钮、未开放功能、属性图标提供说明。

### P4：升级/遗物 Choice Card

目标：

- 升级和遗物三选一更像可读卡片，而不是纯文本列表。

## 七、需要注意的差异

shadcn/ui 是 Web UI，不能直接搬到 Unity。

应该学习的是：

- 组件拆分方式
- 主题 token
- variant 思维
- 状态管理
- 信息层级
- 可组合结构

不应该直接照搬的是：

- Tailwind class
- React 组件结构
- CSS 变量语法
- Web layout 细节

Unity 里要对应成：

- `GameObject + RectTransform`
- `Image`
- `Text / TMP_Text`
- `Button / Toggle / Slider`
- `CanvasGroup`
- `UITheme`
- Prefab 或运行时工厂方法

## 八、结论

shadcn/ui 对当前项目最有价值的不是视觉皮肤，而是“把 UI 做成一套可组合设计系统”的思路。下一步建议先把开始界面按钮和设置页做成 Unity 版 shadcn 风格：统一 token、明确按钮 variant、设置项 Field 化、弹窗 Dialog 化。这样不会删除现有 UI，同时能逐步提升整体完成度。
